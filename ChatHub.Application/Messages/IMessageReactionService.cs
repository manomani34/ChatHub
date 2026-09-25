using ChatHub.Application.Messages.Dtos;

namespace ChatHub.Application.Messages;

public interface IMessageReactionService
{
    Task<List<MessageReactionDto>>
        GetByMessageIdAsync(
            int messageId,
            int currentUserId);

    Task<List<MessageReactionDto>>
        ToggleAsync(
            ToggleMessageReactionDto request,
            int currentUserId);
}