
using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Messages.Dtos;
using ChatHub.Domain.Entities;

namespace ChatHub.Application.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _repository;

    public MessageService(
        IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<MessageDto> SendToConversationAsync(
    SendDirectMessageDto request,
    int senderId)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (senderId <= 0)
            throw new ArgumentException(
                "Invalid sender id.",
                nameof(senderId));

        if (request.ConversationId <= 0)
            throw new ArgumentException(
                "Invalid conversation id.",
                nameof(request.ConversationId));

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(request.Content));

        if (request.ParentMessageId.HasValue)
        {
            var parentMessage =
                await _repository.GetByIdAsync(
                    request.ParentMessageId.Value);

            if (parentMessage is null)
            {
                throw new ArgumentException(
                    "Parent message was not found.",
                    nameof(request.ParentMessageId));
            }

            if (parentMessage.IsDeleted)
            {
                throw new ArgumentException(
                    "Cannot reply to a deleted message.",
                    nameof(request.ParentMessageId));
            }

            if (parentMessage.ConversationId != request.ConversationId ||
                parentMessage.ChannelId != null)
            {
                throw new ArgumentException(
                    "Parent message does not belong to this conversation.",
                    nameof(request.ParentMessageId));
            }
        }

        var message = new Message
        {
            SenderId = senderId,
            ConversationId = request.ConversationId,
            ParentMessageId = request.ParentMessageId,
            ChannelId = null,
            Content = request.Content.Trim(),
            IsEdited = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var createdMessage =
            await _repository.CreateAsync(message);

        return MapToDto(createdMessage);
    }
    public async Task<List<MessageDto>> GetByConversationIdAsync(
    int conversationId)
    {
        if (conversationId <= 0)
            return new List<MessageDto>();

        var messages =
            await _repository.GetByConversationIdAsync(
                conversationId);

        return messages
            .Select(MapToDto)
            .ToList();
    }

    /* =========================================================
       Get Message By Id
       ========================================================= */

    public async Task<MessageDto?> GetByIdAsync(
        int messageId)
    {
        if (messageId <= 0)
        {
            return null;
        }

        var message =
            await _repository.GetByIdAsync(
                messageId);

        if (message is null)
        {
            return null;
        }

        return MapToDto(message);
    }


    /* =========================================================
       Get Messages By Channel
       ========================================================= */

    public async Task<List<MessageDto>> GetByChannelIdAsync(
        int channelId)
    {
        if (channelId <= 0)
        {
            return new List<MessageDto>();
        }

        var messages =
            await _repository.GetByChannelIdAsync(
                channelId);

        return messages
            .Select(MapToDto)
            .ToList();
    }


    /* =========================================================
       Send Message
       ========================================================= */

    public async Task<MessageDto> SendAsync(
        SendMessageDto request,
        int senderId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (senderId <= 0)
        {
            throw new ArgumentException(
                "Invalid sender id.",
                nameof(senderId));
        }

        if (request.ChannelId <= 0)
        {
            throw new ArgumentException(
                "Invalid channel id.",
                nameof(request.ChannelId));
        }

        if (string.IsNullOrWhiteSpace(
                request.Content))
        {
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(request.Content));
        }

        if (request.ParentMessageId.HasValue)
        {
            var parentMessage =
                await _repository.GetByIdAsync(
                    request.ParentMessageId.Value);

            if (parentMessage is null)
            {
                throw new ArgumentException(
                    "Parent message was not found.",
                    nameof(request.ParentMessageId));
            }

            if (parentMessage.IsDeleted)
            {
                throw new ArgumentException(
                    "Cannot reply to a deleted message.",
                    nameof(request.ParentMessageId));
            }

            if (parentMessage.ChannelId != request.ChannelId ||
                parentMessage.ConversationId != null)
            {
                throw new ArgumentException(
                    "Parent message does not belong to this channel.",
                    nameof(request.ParentMessageId));
            }
        }


        var message =
            new Message
            {
                SenderId =
                    senderId,

                ChannelId =
                    request.ChannelId,

                ParentMessageId =
    request.ParentMessageId,

                Content =
                    request.Content.Trim(),

                IsEdited =
                    false,

                IsDeleted =
                    false,

                CreatedAt =
                    DateTime.UtcNow
            };


        var createdMessage =
            await _repository.CreateAsync(
                message);


        return MapToDto(
            createdMessage);
    }


    /* =========================================================
       Edit Message
       ========================================================= */

    public async Task<MessageDto?> EditAsync(
        EditMessageDto request,
        int userId)
    {
        if (request is null)
        {
            return null;
        }

        if (request.MessageId <= 0)
        {
            return null;
        }

        if (userId <= 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(
                request.Content))
        {
            return null;
        }


        var message =
            await _repository.GetByIdAsync(
                request.MessageId);


        if (message is null)
        {
            return null;
        }


        /* -----------------------------------------------------
           Ownership
           ----------------------------------------------------- */

        if (message.SenderId != userId)
        {
            return null;
        }


        /* -----------------------------------------------------
           Deleted messages cannot be edited
           ----------------------------------------------------- */

        if (message.IsDeleted)
        {
            return null;
        }


        message.Content =
            request.Content.Trim();

        message.IsEdited =
            true;

        message.UpdatedAt =
            DateTime.UtcNow;


        var updatedMessage =
            await _repository.UpdateAsync(
                message);


        return MapToDto(
            updatedMessage);
    }


    /* =========================================================
       Delete Message
       ========================================================= */

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
            await _repository.GetByIdAsync(
                messageId);


        if (message is null)
        {
            return null;
        }


        /* -----------------------------------------------------
           Ownership
           ----------------------------------------------------- */

        if (message.SenderId != userId)
        {
            return null;
        }


        /* -----------------------------------------------------
           Already deleted
           ----------------------------------------------------- */

        if (message.IsDeleted)
        {
            return null;
        }


        message.IsDeleted =
            true;

        message.UpdatedAt =
            DateTime.UtcNow;


        var updatedMessage =
            await _repository.UpdateAsync(
                message);


        return MapToDto(
            updatedMessage);
    }


    /* =========================================================
       Mapping
       ========================================================= */

    private static MessageDto MapToDto(
        Message message)
    {
        return new MessageDto
        {
            Id =
                message.Id,

            SenderId =
                message.SenderId,

            SenderName =
                message.Sender.DisplayName,

            SenderAvatarUrl =
                message.Sender.AvatarUrl,

            ChannelId =
                message.ChannelId,

            ConversationId =
                message.ConversationId,

            ParentMessageId =
    message.ParentMessageId,

            Content =
                message.Content,

            IsEdited =
                message.IsEdited,

            IsDeleted =
                message.IsDeleted,

            CreatedAt =
                message.CreatedAt,

            UpdatedAt =
                message.UpdatedAt
        };
    }
}
