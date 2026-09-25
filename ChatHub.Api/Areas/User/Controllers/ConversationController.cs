using ChatHub.Api.Hubs;
using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Conversations;
using ChatHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;


namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/conversations")]
public class ConversationController : ControllerBase
{
    private readonly IDirectMessageService _directMessageService;
    private readonly IHubContext<ChatHubHub> _hubContext;
    private readonly IConversationReadStateRepository _readStateRepository;
    private readonly IConversationRepository _conversationRepository;

    public ConversationController(
     IDirectMessageService directMessageService,
     IHubContext<ChatHubHub> hubContext,
     IConversationReadStateRepository readStateRepository,
     IConversationRepository conversationRepository)
    {
        _directMessageService = directMessageService;
        _hubContext = hubContext;
        _readStateRepository = readStateRepository;
        _conversationRepository = conversationRepository;
    }



    [HttpGet("direct/open")]
    public async Task<IActionResult> GetOrCreateDirectConversation(
        [FromQuery] string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest();

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null ||
            !int.TryParse(userIdClaim.Value, out var currentUserId) ||
            currentUserId <= 0)
        {
            return Unauthorized();
        }

        var conversation =
            await _directMessageService
                .GetOrCreateConversationAsync(
                    currentUserId,
                    userName);

        if (conversation is null)
            return NotFound();

        await _hubContext.Clients
    .Group($"user-{currentUserId}")
    .SendAsync(
        "DirectConversationAvailable",
        conversation);

        await _hubContext.Clients
            .Group($"user-{conversation.OtherUserId}")
            .SendAsync(
                "DirectConversationAvailable",
                conversation);

        return Ok(conversation);
    }

    [HttpGet("direct")]
    public async Task<IActionResult> GetDirectConversations()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var conversations =
            await _directMessageService
                .GetDirectConversationsAsync(
                    userId.Value);

        return Ok(conversations);
    }

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

    [HttpGet("{conversationId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(
    int conversationId)
    {
        var userId = GetCurrentUserId();

        if (userId is null || userId <= 0)
            return Unauthorized();

        if (conversationId <= 0)
            return BadRequest();

        var isMember =
            await _conversationRepository.IsConversationMemberAsync(
                conversationId,
                userId.Value);

        if (!isMember)
            return NotFound();

        var count =
            await _readStateRepository.GetUnreadCountAsync(
                userId.Value,
                conversationId);

        return Ok(new
        {
            conversationId,
            unreadCount = count
        });
    }
    [HttpPost("{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(
    int conversationId,
    [FromQuery] int? lastReadMessageId)
    {
        var userId = GetCurrentUserId();

        if (userId is null || userId <= 0)
            return Unauthorized();

        if (conversationId <= 0)
            return BadRequest();

        if (lastReadMessageId.HasValue &&
            lastReadMessageId.Value <= 0)
        {
            return BadRequest();
        }

        var isMember =
            await _conversationRepository.IsConversationMemberAsync(
                conversationId,
                userId.Value);

        if (!isMember)
            return NotFound();

        await _readStateRepository.MarkAsReadAsync(
            userId.Value,
            conversationId,
            lastReadMessageId);

        return NoContent();
    }
}