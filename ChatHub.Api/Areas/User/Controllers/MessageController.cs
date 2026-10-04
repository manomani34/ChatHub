using System.Security.Claims;

using ChatHub.Api.Hubs;

using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Messages;
using ChatHub.Application.Messages.Dtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/messages")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;
    private readonly IHubContext<ChatHubHub> _hubContext;
    private readonly IMessageRepository _messageRepository;

    public MessageController(
        IMessageService messageService,
        IWorkspaceMembershipRepository membershipRepository,
        IConversationRepository conversationRepository,
        IHubContext<ChatHubHub> hubContext,
        IMessageRepository messageRepository)
    {
        _messageService = messageService;
        _membershipRepository = membershipRepository;
        _conversationRepository = conversationRepository;
        _hubContext = hubContext;
        _messageRepository = messageRepository;
    }

    /* =========================================================
       Send Direct Message
       ========================================================= */

    [HttpPost("conversation")]
    public async Task<IActionResult> SendDirectMessage(
        [FromBody] SendDirectMessageDto request)
    {
        if (request is null)
            return BadRequest();

        if (request.ConversationId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest();

        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var isMember =
            await _conversationRepository.IsMemberAsync(
                request.ConversationId,
                userId.Value);

        if (!isMember)
            return NotFound();

        var message =
            await _messageService.SendToConversationAsync(
                request,
                userId.Value);

        var memberUserIds =
            await _conversationRepository.GetMemberUserIdsAsync(
                request.ConversationId);

        foreach (var memberUserId in memberUserIds)
        {
            if (memberUserId == userId.Value)
                continue;

            await _hubContext.Clients
                .Group($"user-{memberUserId}")
                .SendAsync(
                    "DirectUnreadUpdated",
                    new
                    {
                        conversationId = request.ConversationId,
                        lastMessageContent = message.Content,
                        lastMessageAt = message.CreatedAt
                    });
        }

        await _hubContext.Clients
            .Group($"conversation-{request.ConversationId}")
            .SendAsync(
                "ReceiveDirectMessage",
                message);

        return Ok(message);
    }

    /* =========================================================
       Current User
       ========================================================= */

    private int? GetCurrentUserId()
    {
        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (
            claim is null ||
            !int.TryParse(
                claim.Value,
                out var userId) ||
            userId <= 0)
        {
            return null;
        }

        return userId;
    }

    /* =========================================================
       Get Messages - Channel
       ========================================================= */

    [HttpGet("channel/{channelId:int}")]
    public async Task<IActionResult> GetByChannel(
        int channelId)
    {
        if (channelId <= 0)
        {
            return BadRequest(
                "Invalid channel id.");
        }

        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var isMember =
            await _membershipRepository
                .IsMemberOfChannelAsync(
                    channelId,
                    userId.Value);

        if (!isMember)
        {
            return Forbid();
        }

        var messages =
            await _messageService
                .GetByChannelIdAsync(
                    channelId);

        return Ok(messages);
    }

    /* =========================================================
       Get Messages - Conversation
       ========================================================= */

    [HttpGet("conversation/{conversationId:int}")]
    public async Task<IActionResult> GetConversationMessages(
        int conversationId)
    {
        if (conversationId <= 0)
            return BadRequest();

        var userId =
            GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var isMember =
            await _conversationRepository.IsMemberAsync(
                conversationId,
                userId.Value);

        if (!isMember)
            return NotFound();

        var messages =
            await _messageService
                .GetByConversationIdAsync(
                    conversationId);

        return Ok(messages);
    }

    /* =========================================================
    Search Messages
    ========================================================= */

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int? channelId = null,
        [FromQuery] int? conversationId = null,
        [FromQuery] int take = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required.");

        if (channelId.HasValue && channelId.Value <= 0)
            return BadRequest("Invalid channel id.");

        if (conversationId.HasValue && conversationId.Value <= 0)
            return BadRequest("Invalid conversation id.");

        // Exactly one target must be specified.
        if (!channelId.HasValue && !conversationId.HasValue)
            return BadRequest(
                "Specify a channelId or conversationId.");

        if (channelId.HasValue && conversationId.HasValue)
            return BadRequest(
                "Specify either channelId or conversationId, not both.");

        if (take <= 0)
            take = 50;

        if (take > 100)
            take = 100;

        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        /* -----------------------------------------------------
           Channel Search
           ----------------------------------------------------- */

        if (channelId.HasValue)
        {
            var isMember =
                await _membershipRepository
                    .IsMemberOfChannelAsync(
                        channelId.Value,
                        userId.Value);

            if (!isMember)
                return Forbid();
        }

        /* -----------------------------------------------------
           Conversation Search
           ----------------------------------------------------- */

        if (conversationId.HasValue)
        {
            var isMember =
                await _conversationRepository
                    .IsMemberAsync(
                        conversationId.Value,
                        userId.Value);

            if (!isMember)
                return NotFound();
        }

        var messages =
            await _messageService.SearchAsync(
                query,
                channelId,
                conversationId,
                take);

        return Ok(messages);
    }

    /* =========================================================
       Send Channel Message
       ========================================================= */

    [HttpPost]
    public async Task<IActionResult> Send(
        [FromBody] SendMessageDto request)
    {
        if (request is null)
        {
            return BadRequest(
                "Message request is required.");
        }

        if (request.ChannelId <= 0)
        {
            return BadRequest(
                "ChannelId is required.");
        }

        if (
            string.IsNullOrWhiteSpace(
                request.Content))
        {
            return BadRequest(
                "Message content cannot be empty.");
        }

        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var isMember =
            await _membershipRepository
                .IsMemberOfChannelAsync(
                    request.ChannelId,
                    userId.Value);

        if (!isMember)
        {
            return Forbid();
        }

        var message =
            await _messageService.SendAsync(
                request,
                userId.Value);

        await _hubContext.Clients
            .Group(
                $"channel-{message.ChannelId}")
            .SendAsync(
                "ReceiveMessage",
                message);

        return Ok(message);
    }

    /* =========================================================
       Edit Message
       ========================================================= */

    [HttpPut("{messageId:int}")]
    public async Task<IActionResult> Edit(
        int messageId,
        [FromBody] EditMessageDto request)
    {
        if (messageId <= 0)
        {
            return BadRequest(
                "Invalid message id.");
        }

        if (
            request is null ||
            string.IsNullOrWhiteSpace(
                request.Content))
        {
            return BadRequest(
                "Message content cannot be empty.");
        }

        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var existingMessage =
            await _messageService.GetByIdAsync(
                messageId);

        if (existingMessage is null)
        {
            return NotFound(
                "Message not found.");
        }

        /* -----------------------------------------------------
           Channel Message
           ----------------------------------------------------- */

        if (existingMessage.ChannelId.HasValue)
        {
            var isMember =
                await _membershipRepository
                    .IsMemberOfChannelAsync(
                        existingMessage.ChannelId.Value,
                        userId.Value);

            if (!isMember)
            {
                return Forbid();
            }
        }

        /* -----------------------------------------------------
           Direct Message
           ----------------------------------------------------- */

        else if (existingMessage.ConversationId.HasValue)
        {
            var isMember =
                await _conversationRepository
                    .IsMemberAsync(
                        existingMessage.ConversationId.Value,
                        userId.Value);

            if (!isMember)
            {
                return Forbid();
            }
        }

        /* -----------------------------------------------------
           Invalid Message Target
           ----------------------------------------------------- */

        else
        {
            return BadRequest(
                "Message is not associated with a channel or conversation.");
        }

        request.MessageId =
            messageId;

        var message =
            await _messageService.EditAsync(
                request,
                userId.Value);

        if (message is null)
        {
            return NotFound(
                "Message not found or you are not allowed to edit it.");
        }

        /* -----------------------------------------------------
           Broadcast Message Edit
           ----------------------------------------------------- */

        if (message.ChannelId.HasValue)
        {
            await _hubContext.Clients
                .Group(
                    $"channel-{message.ChannelId}")
                .SendAsync(
                    "MessageEdited",
                    message);
        }
        else if (message.ConversationId.HasValue)
        {
            await _hubContext.Clients
                .Group(
                    $"conversation-{message.ConversationId}")
                .SendAsync(
                    "MessageEdited",
                    message);

            /*
             * Update direct conversation sidebar
             *
             * The helper fetches the actual latest message
             * from the database, so editing an old message
             * will not incorrectly change the preview.
             */
            await BroadcastDirectConversationUpdatedAsync(
                message.ConversationId.Value);
        }

        return Ok(message);
    }

    /* =========================================================
       Delete Message
       ========================================================= */

    [HttpDelete("{messageId:int}")]
    public async Task<IActionResult> Delete(
        int messageId)
    {
        if (messageId <= 0)
        {
            return BadRequest(
                "Invalid message id.");
        }

        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var existingMessage =
            await _messageService.GetByIdAsync(
                messageId);

        if (existingMessage is null)
        {
            return NotFound(
                "Message not found.");
        }

        /* -----------------------------------------------------
           Channel Message
           ----------------------------------------------------- */

        if (existingMessage.ChannelId.HasValue)
        {
            var isMember =
                await _membershipRepository
                    .IsMemberOfChannelAsync(
                        existingMessage.ChannelId.Value,
                        userId.Value);

            if (!isMember)
            {
                return Forbid();
            }
        }

        /* -----------------------------------------------------
           Direct Message
           ----------------------------------------------------- */

        else if (existingMessage.ConversationId.HasValue)
        {
            var isMember =
                await _conversationRepository
                    .IsMemberAsync(
                        existingMessage.ConversationId.Value,
                        userId.Value);

            if (!isMember)
            {
                return Forbid();
            }
        }

        /* -----------------------------------------------------
           Invalid Message Target
           ----------------------------------------------------- */

        else
        {
            return BadRequest(
                "Message is not associated with a channel or conversation.");
        }

        var deletedMessage =
            await _messageService.DeleteAsync(
                messageId,
                userId.Value);

        if (deletedMessage is null)
        {
            return NotFound(
                "Message not found or you are not allowed to delete it.");
        }

        /* -----------------------------------------------------
           Broadcast Message Delete
           ----------------------------------------------------- */

        if (deletedMessage.ChannelId.HasValue)
        {
            await _hubContext.Clients
                .Group(
                    $"channel-{deletedMessage.ChannelId}")
                .SendAsync(
                    "MessageDeleted",
                    deletedMessage);
        }
        else if (deletedMessage.ConversationId.HasValue)
        {
            await _hubContext.Clients
                .Group(
                    $"conversation-{deletedMessage.ConversationId}")
                .SendAsync(
                    "MessageDeleted",
                    deletedMessage);

            /*
             * Recalculate sidebar preview.
             *
             * If this was the latest message,
             * the previous message becomes the preview.
             * If there is no remaining message,
             * preview/time become empty.
             */
            await BroadcastDirectConversationUpdatedAsync(
                deletedMessage.ConversationId.Value);
        }

        return Ok(deletedMessage);
    }

    /* =========================================================
       Direct Conversation Sidebar Update
       ========================================================= */

    private async Task BroadcastDirectConversationUpdatedAsync(
        int conversationId)
    {
        if (conversationId <= 0)
            return;

        var lastMessage =
            await _messageRepository
                .GetLastMessageByConversationIdAsync(
                    conversationId);

        var memberUserIds =
            await _conversationRepository
                .GetMemberUserIdsAsync(
                    conversationId);

        foreach (var memberUserId in memberUserIds)
        {
            await _hubContext.Clients
                .Group(
                    $"user-{memberUserId}")
                .SendAsync(
                    "DirectConversationUpdated",
                    new
                    {
                        conversationId,
                        lastMessageContent =
                            lastMessage?.Content,
                        lastMessageAt =
                            lastMessage?.CreatedAt,
                        moveToTop = false
                    });
        }
    }
}