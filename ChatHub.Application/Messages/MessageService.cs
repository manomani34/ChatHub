using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Messages.Dtos;
using ChatHub.Domain.Entities;

namespace ChatHub.Application.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _repository;

    public MessageService(IMessageRepository repository)
    {
        _repository = repository;
    }


    public async Task<List<MessageDto>> GetByChannelIdAsync(int channelId)
    {
        var messages = await _repository
            .GetByChannelIdAsync(channelId);

        return messages
            .Select(MapToDto)
            .ToList();
    }

    public async Task<MessageDto> SendAsync(SendMessageDto request)
    {
        var message = new Message
        {
            SenderId = request.SenderId,
            ChannelId = request.ChannelId,
            Content = request.Content.Trim(),
            IsEdited = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var createdMessage = await _repository
            .CreateAsync(message);

        return MapToDto(createdMessage);
    }

    private static MessageDto MapToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender.DisplayName,
            SenderAvatarUrl = message.Sender.AvatarUrl,
            ChannelId = message.ChannelId,
            ConversationId = message.ConversationId,
            Content = message.Content,
            IsEdited = message.IsEdited,
            IsDeleted = message.IsDeleted,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }
    public async Task<MessageDto?> EditAsync(EditMessageDto request)
    {
        if (request.MessageId <= 0)
        {
            return null;
        }

        if (request.UserId <= 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return null;
        }

        var message =
            await _repository.GetByIdAsync(
                request.MessageId);

        Console.WriteLine(
    $"EDIT DEBUG => MessageId={request.MessageId}, UserId={request.UserId}");

        Console.WriteLine(
            $"EDIT DEBUG => Found={message != null}, SenderId={message?.SenderId}, IsDeleted={message?.IsDeleted}");

        if (message is null)
        {
            return null;
        }

        // فقط صاحب پیام می‌تواند آن را ویرایش کند
        if (message.SenderId != request.UserId)
        {
            return null;
        }

        if (message.IsDeleted)
        {
            return null;
        }

        message.Content =
            request.Content.Trim();

        message.IsEdited = true;
        message.UpdatedAt = DateTime.UtcNow;

        var updatedMessage =
            await _repository.UpdateAsync(message);

        return MapToDto(updatedMessage);
    }

    public async Task<MessageDto?> DeleteAsync(
     int messageId,
     int userId)
    {
        if (messageId <= 0)
        {
            return null;
        }

        if (userId <= 0)
        {
            return null;
        }

        var message =
            await _repository.GetByIdAsync(messageId);

        if (message is null)
        {
            return null;
        }

        // فقط صاحب پیام می‌تواند آن را حذف کند
        if (message.SenderId != userId)
        {
            return null;
        }

        if (message.IsDeleted)
        {
            return null;
        }

        message.IsDeleted = true;
        message.UpdatedAt = DateTime.UtcNow;

        var updatedMessage =
            await _repository.UpdateAsync(message);

        return MapToDto(updatedMessage);
    }
}