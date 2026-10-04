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

    public async Task<List<WorkspaceMemberDto>> GetWorkspaceMembersAsync(
     int workspaceId)
    {
        if (workspaceId <= 0)
            return new List<WorkspaceMemberDto>();

        var response =
            await _httpClient.GetAsync(
                $"api/user/workspaces/{workspaceId}/members");

        if (!response.IsSuccessStatusCode)
            return new List<WorkspaceMemberDto>();

        return await response.Content
            .ReadFromJsonAsync<List<WorkspaceMemberDto>>()
            ?? new List<WorkspaceMemberDto>();
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(
    string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        var response =
            await _httpClient.GetAsync(
                $"api/user/profile?userName={Uri.EscapeDataString(userName)}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<UserProfileResponse>();
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
    int channelId,
    string content,
    int? parentMessageId = null)
    {
        var request = new SendMessageRequest
        {
            ChannelId = channelId,
            ParentMessageId = parentMessageId,
            Content = content
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/user/messages",
                request);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

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
    string content)
    {
        var request = new
        {
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
    int messageId)
    {
        var response =
            await _httpClient.DeleteAsync(
                $"api/user/messages/{messageId}");

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

    public async Task<DirectConversationResponse?> GetOrCreateDirectConversationAsync(
    string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        var response =
            await _httpClient.GetAsync(
                $"api/user/conversations/direct/open?userName={Uri.EscapeDataString(userName)}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<DirectConversationResponse>();
    }


    public async Task<List<MessageDto>> GetConversationMessagesAsync(
        int conversationId)
    {
        if (conversationId <= 0)
            return new List<MessageDto>();

        var response =
            await _httpClient.GetAsync(
                $"api/user/messages/conversation/{conversationId}");

        if (!response.IsSuccessStatusCode)
            return new List<MessageDto>();

        return await response.Content
            .ReadFromJsonAsync<List<MessageDto>>()
            ?? new List<MessageDto>();
    }

    public async Task<List<MessageDto>> SearchMessagesAsync(
    string query,
    int? channelId = null,
    int? conversationId = null,
    int take = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<MessageDto>();

        var parameters = new List<string>
    {
        $"query={Uri.EscapeDataString(query)}"
    };

        if (channelId.HasValue)
            parameters.Add($"channelId={channelId.Value}");

        if (conversationId.HasValue)
            parameters.Add($"conversationId={conversationId.Value}");

        if (take > 0)
            parameters.Add($"take={take}");

        var url =
            $"api/user/messages/search?{string.Join("&", parameters)}";

        var response =
            await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<MessageDto>();

        return await response.Content
            .ReadFromJsonAsync<List<MessageDto>>()
            ?? new List<MessageDto>();
    }

    public async Task<MessageDto?> SendDirectMessageAsync(
     int conversationId,
     string content,
     int? parentMessageId = null)
    {
        if (conversationId <= 0)
            return null;

        if (string.IsNullOrWhiteSpace(content))
            return null;

        var request = new
        {
            conversationId,
            parentMessageId,
            content
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/user/messages/conversation",
                request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<MessageDto>();
    }

    public async Task<List<DirectConversationResponse>>
    GetDirectConversationsAsync()
    {
        var response =
            await _httpClient.GetAsync(
                "api/user/conversations/direct");

        if (!response.IsSuccessStatusCode)
        {
            return new List<DirectConversationResponse>();
        }

        return await response.Content
            .ReadFromJsonAsync<List<DirectConversationResponse>>()
            ?? new List<DirectConversationResponse>();
    }
    
}

public class DirectConversationResponse
{
    public int ConversationId { get; set; }

    public int OtherUserId { get; set; }

    public string OtherUserName { get; set; }
        = string.Empty;

    public string OtherDisplayName { get; set; }
        = string.Empty;

    public string? OtherAvatarUrl { get; set; }

    public int UnreadCount { get; set; }

    public string? LastMessageContent { get; set; }

    public DateTime? LastMessageAt { get; set; }
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
    public int? ParentMessageId { get; set; }

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
    public int? ParentMessageId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UserProfileResponse
{
    public int UserId { get; set; }

    public string UserName { get; set; } =
        string.Empty;

    public string DisplayName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;
}
public class WorkspaceMemberDto
{
    public int UserId { get; set; }

    public string UserName { get; set; }
        = string.Empty;

    public string DisplayName { get; set; }
        = string.Empty;

    public string Email { get; set; }
        = string.Empty;

    public string? AvatarUrl { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public bool IsActive { get; set; }
}
