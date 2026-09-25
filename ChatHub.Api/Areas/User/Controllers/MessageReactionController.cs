using System.Security.Claims;

using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Messages;
using ChatHub.Application.Messages.Dtos;
using ChatHub.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/messages")]
public class MessageReactionController : ControllerBase
{
    private readonly IMessageReactionService
        _reactionService;

    private readonly IMessageService
        _messageService;

    private readonly IWorkspaceMembershipRepository
        _membershipRepository;

    private readonly IConversationRepository
        _conversationRepository;

    private readonly IHubContext<ChatHubHub>
    _hubContext;

    public MessageReactionController(
    IMessageReactionService reactionService,
    IMessageService messageService,
    IWorkspaceMembershipRepository membershipRepository,
    IConversationRepository conversationRepository,
    IHubContext<ChatHubHub> hubContext)
    {
        _reactionService =
            reactionService;

        _messageService =
            messageService;

        _membershipRepository =
            membershipRepository;

        _conversationRepository =
            conversationRepository;

        _hubContext =
            hubContext;
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
       Get Message Reactions
       ========================================================= */

    [HttpGet("{messageId:int}/reactions")]
    public async Task<IActionResult> GetReactions(
        int messageId)
    {
        if (messageId <= 0)
            return BadRequest();

        var userId =
            GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var message =
            await _messageService
                .GetByIdAsync(messageId);

        if (message is null)
            return NotFound();

        /*
         * Channel Message
         */

        if (message.ChannelId.HasValue)
        {
            var isMember =
                await _membershipRepository
                    .IsMemberOfChannelAsync(
                        message.ChannelId.Value,
                        userId.Value);

            if (!isMember)
                return Forbid();
        }

        /*
         * Direct Message
         */

        else if (message.ConversationId.HasValue)
        {
            var isMember =
                await _conversationRepository
                    .IsMemberAsync(
                        message.ConversationId.Value,
                        userId.Value);

            if (!isMember)
                return Forbid();
        }

        /*
         * Invalid target
         */

        else
        {
            return BadRequest(
                "Message is not associated with a channel or conversation.");
        }

        var reactions =
            await _reactionService
                .GetByMessageIdAsync(
                    messageId,
                    userId.Value);

        return Ok(reactions);
    }

    /* =========================================================
       Toggle Message Reaction
       ========================================================= */

    [HttpPost("reactions")]
    public async Task<IActionResult> ToggleReaction(
        [FromBody] ToggleMessageReactionDto request)
    {
        if (request is null)
            return BadRequest();

        if (request.MessageId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(request.Emoji))
            return BadRequest();

        var userId =
            GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var message =
            await _messageService
                .GetByIdAsync(
                    request.MessageId);

        if (message is null)
            return NotFound();

        /*
         * Channel Message
         */

        if (message.ChannelId.HasValue)
        {
            var isMember =
                await _membershipRepository
                    .IsMemberOfChannelAsync(
                        message.ChannelId.Value,
                        userId.Value);

            if (!isMember)
                return Forbid();
        }

        /*
         * Direct Message
         */

        else if (message.ConversationId.HasValue)
        {
            var isMember =
                await _conversationRepository
                    .IsMemberAsync(
                        message.ConversationId.Value,
                        userId.Value);

            if (!isMember)
                return Forbid();
        }

        /*
         * Invalid target
         */

        else
        {
            return BadRequest(
                "Message is not associated with a channel or conversation.");
        }

        var reactions =
     await _reactionService
         .ToggleAsync(
             request,
             userId.Value);


        /* ---------------------------------------------------------
           Broadcast Reaction Change
           --------------------------------------------------------- */

        if (message.ChannelId.HasValue)
        {
            await _hubContext.Clients
                .Group(
                    $"channel-{message.ChannelId.Value}")
                .SendAsync(
                    "MessageReactionUpdated",
                    new
                    {
                        messageId =
                            message.Id,

                        channelId =
                            message.ChannelId,

                        conversationId =
                            (int?)null
                    });
        }
        else if (message.ConversationId.HasValue)
        {
            await _hubContext.Clients
                .Group(
                    $"conversation-{message.ConversationId.Value}")
                .SendAsync(
                    "MessageReactionUpdated",
                    new
                    {
                        messageId =
                            message.Id,

                        channelId =
                            (int?)null,

                        conversationId =
                            message.ConversationId
                    });
        }


        return Ok(
            new
            {
                messageId =
                    request.MessageId,

                reactions
            });
    }
}