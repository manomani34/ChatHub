using ChatHub.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatHub.Web.Areas.User.Controllers;

[Area("User")]
[Authorize]
public class HomeController : Controller
{
    private readonly ChatHubApiClient _apiClient;

    public HomeController(ChatHubApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(
    int? channelId,
    string? userName)
    {
        var workspaces =
            await _apiClient.GetWorkspacesAsync();

        var workspace =
            workspaces.FirstOrDefault();

        if (workspace is null)
        {
            return View(new ChatHubHomeViewModel());
        }

        var currentUserId =
     User.FindFirst(
         System.Security.Claims.ClaimTypes.NameIdentifier)
     ?.Value;

        var members =
            await _apiClient.GetWorkspaceMembersAsync(
                workspace.Id);

        var directConversations =
    await _apiClient.GetDirectConversationsAsync();                

        if (int.TryParse(currentUserId, out var parsedUserId))
        {
            members = members
                .Where(x => x.UserId != parsedUserId)
                .ToList();
        }


        var channels =
            await _apiClient.GetChannelsAsync(
                workspace.Id);

        var model = new ChatHubHomeViewModel
        {
            Workspace = workspace,
            Channels = channels,
            Members = members,
            DirectConversations = directConversations
        };


        // =====================================================
        // Direct Message
        // =====================================================

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var directUser =
                members.FirstOrDefault(x =>
                    string.Equals(
                        x.UserName,
                        userName,
                        StringComparison.OrdinalIgnoreCase));

            if (directUser is null)
            {
                return NotFound();
            }

            var conversation =
                await _apiClient
                    .GetOrCreateDirectConversationAsync(
                        directUser.UserName);

            if (conversation is null)
            {
                return NotFound();
            }

            var messages =
                await _apiClient
                    .GetConversationMessagesAsync(
                        conversation.ConversationId);

            model.IsDirectMessage = true;

            model.SelectedConversationId =
                conversation.ConversationId;

            model.DirectMessageUser =
                directUser;

            model.Messages =
                messages;

            return View(model);
        }


        // =====================================================
        // Channel
        // =====================================================

        var selectedChannel =
            channels.FirstOrDefault();

        if (channelId.HasValue)
        {
            selectedChannel =
                channels.FirstOrDefault(
                    x => x.Id == channelId.Value)
                ?? selectedChannel;
        }

        var channelMessages =
            new List<MessageDto>();

        if (selectedChannel is not null)
        {
            channelMessages =
                await _apiClient
                    .GetMessagesAsync(
                        selectedChannel.Id);
        }

        model.SelectedChannel =
            selectedChannel;

        model.Messages =
            channelMessages;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(
    int channelId,
    string content,
    int? parentMessageId)
    {
        if (channelId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(content))
            return BadRequest();

        await _apiClient.SendMessageAsync(
            channelId,
            content,
            parentMessageId);

        return RedirectToAction(
            nameof(Index),
            new { channelId });
    }

    [HttpPost]
    public async Task<IActionResult> SendDirectMessage(
    int conversationId,
    string content,
    string? userName,
    int? parentMessageId)
    {
        if (conversationId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(content))
            return BadRequest();

        Console.WriteLine(
    $"DEBUG REPLY: conversationId={conversationId}, parentMessageId={parentMessageId}, content={content}"
);

        var message =
            await _apiClient.SendDirectMessageAsync(
                conversationId,
                content,
                parentMessageId);

        if (message is null)
            return BadRequest();

        return RedirectToAction(
            nameof(Index),
            new { userName });
    }

    [HttpPost]
    public async Task<IActionResult> EditMessage(
     int messageId,
     string content)
    {
        if (messageId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(content))
            return BadRequest();

        var message =
            await _apiClient.EditMessageAsync(
                messageId,
                content);

        if (message is null)
            return NotFound();

        return Ok(message);
    }


    [HttpPost]
    public async Task<IActionResult> DeleteMessage(
        int messageId)
    {
        if (messageId <= 0)
            return BadRequest();

        var message =
            await _apiClient.DeleteMessageAsync(
                messageId);

        if (message is null)
            return NotFound();

        return Ok(message);
    }
}

public class ChatHubHomeViewModel
{
    public WorkspaceDto? Workspace { get; set; }

    public List<ChannelDto> Channels { get; set; }
        = new();

    public ChannelDto? SelectedChannel { get; set; }

    public List<MessageDto> Messages { get; set; }
        = new();
    public List<WorkspaceMemberDto> Members { get; set; }
    = new();
    public bool IsDirectMessage { get; set; }

    public int? SelectedConversationId { get; set; }

    public WorkspaceMemberDto? DirectMessageUser { get; set; }
    public List<DirectConversationResponse> DirectConversations { get; set; }
    = new();
}