using ChatHub.Application.Conversations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/conversations")]
public class ConversationReadStateController : ControllerBase
{
    private readonly IConversationReadStateService _service;

    public ConversationReadStateController(
        IConversationReadStateService service)
    {
        _service = service;
    }

    [HttpGet("{conversationId:int}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(
        int conversationId)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var count =
            await _service.GetUnreadCountAsync(
                userId,
                conversationId);

        return Ok(new
        {
            conversationId,
            unreadCount = count
        });
    }

    [HttpPost("{conversationId:int}/mark-read")]
    public async Task<IActionResult> MarkAsRead(
        int conversationId,
        [FromBody] MarkConversationReadRequest request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _service.MarkAsReadAsync(
            userId,
            conversationId,
            request.LastReadMessageId);

        return Ok(new
        {
            conversationId,
            lastReadMessageId =
                request.LastReadMessageId
        });
    }
}

public class MarkConversationReadRequest
{
    public int? LastReadMessageId { get; set; }
}