using ChatHub.Api.Hubs;
using ChatHub.Application.Messages;
using ChatHub.Application.Messages.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Route("api/user/messages")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHubHub> _hubContext;

    public MessageController(
        IMessageService messageService,
        IHubContext<ChatHubHub> hubContext)
    {
        _messageService = messageService;
        _hubContext = hubContext;
    }

    [HttpGet("channel/{channelId:int}")]
    public async Task<IActionResult> GetByChannel(int channelId)
    {
        var messages = await _messageService
            .GetByChannelIdAsync(channelId);

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Send(
        [FromBody] SendMessageDto request)
    {
        if (request.ChannelId <= 0)
        {
            return BadRequest("ChannelId is required.");
        }

        if (request.SenderId <= 0)
        {
            return BadRequest("SenderId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Message content cannot be empty.");
        }

        var message = await _messageService
    .SendAsync(request);

        await _hubContext.Clients
            .Group($"channel-{message.ChannelId}")
            .SendAsync(
                "ReceiveMessage",
                message);

        return Ok(message);
    }

    [HttpPut("{messageId:int}")]
    public async Task<IActionResult> Edit(
    int messageId,
    [FromBody] EditMessageDto request)
    {
        if (messageId <= 0)
        {
            return BadRequest("Invalid message id.");
        }

        if (request.UserId <= 0)
        {
            return BadRequest("UserId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Message content cannot be empty.");
        }

        request.MessageId = messageId;

        var message =
            await _messageService.EditAsync(request);

        if (message is null)
        {
            return NotFound(
                "Message not found or you are not allowed to edit it.");
        }

        await _hubContext.Clients
            .Group($"channel-{message.ChannelId}")
            .SendAsync(
                "MessageEdited",
                message);

        return Ok(message);
    }

    [HttpDelete("{messageId:int}")]
    public async Task<IActionResult> Delete(
     int messageId,
     [FromQuery] int userId)
    {
        if (messageId <= 0)
        {
            return BadRequest("Invalid message id.");
        }

        if (userId <= 0)
        {
            return BadRequest("UserId is required.");
        }

        var deletedMessage =
            await _messageService.DeleteAsync(
                messageId,
                userId);

        if (deletedMessage is null)
        {
            return NotFound(
                "Message not found or you are not allowed to delete it.");
        }

        await _hubContext.Clients
            .Group($"channel-{deletedMessage.ChannelId}")
            .SendAsync(
                "MessageDeleted",
                deletedMessage);

        return Ok(deletedMessage);
    }
}