using ChatHub.Application.Messages.Dtos;
using System.Net.Http.Json;
using ChatHub.Application.Authentication.Dtos;

namespace ChatHub.Web.Services;

public class ChatHubApiClient
{
    private readonly HttpClient _httpClient;

    public ChatHubApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<WorkspaceDto>> GetWorkspacesAsync()
    {
        var result = await _httpClient
            .GetFromJsonAsync<List<WorkspaceDto>>(
                "api/user/workspaces");

        return result ?? new List<WorkspaceDto>();
    }

    public async Task<List<ChannelDto>> GetChannelsAsync(int workspaceId)
    {
        var result = await _httpClient
            .GetFromJsonAsync<List<ChannelDto>>(
                $"api/user/channels/workspace/{workspaceId}");

        return result ?? new List<ChannelDto>();
    }

    public async Task<List<MessageDto>> GetMessagesAsync(int channelId)
    {
        var result = await _httpClient
            .GetFromJsonAsync<List<MessageDto>>(
                $"api/user/messages/channel/{channelId}");

        return result ?? new List<MessageDto>();
    }

    public async Task<MessageDto?> SendMessageAsync(
    int senderId,
    int channelId,
    string content)
    {
        var request = new SendMessageRequest
        {
            SenderId = senderId,
            ChannelId = channelId,
            Content = content
        };

        var response = await _httpClient
            .PostAsJsonAsync(
                "api/user/messages",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Send message failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<MessageDto>();
    }

    public async Task<MessageDto?> EditMessageAsync(
    int messageId,
    int userId,
    string content)
    {
        var request = new
        {
            UserId = userId,
            Content = content
        };

        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/user/messages/{messageId}",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Edit message failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<MessageDto>();
    }

    public async Task<MessageDto?> DeleteMessageAsync(
    int messageId,
    int userId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/user/messages/{messageId}?userId={userId}");

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Delete message failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<MessageDto>();
    }
    public async Task<LoginResponse?> LoginAsync(
    string userName,
    string password)
    {
        var request = new LoginRequest
        {
            UserName = userName,
            Password = password
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<LoginResponse>();
    }
}

public class WorkspaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ChannelDto
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; }
    public int SortOrder { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string? SenderAvatarUrl { get; set; }

    public int? ChannelId { get; set; }

    public int? ConversationId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsEdited { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class SendMessageRequest
{
    public int SenderId { get; set; }

    public int ChannelId { get; set; }

    public string Content { get; set; } = string.Empty;
}