using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IMessageReactionRepository
{
    Task<List<MessageReaction>> GetByMessageIdAsync(
        int messageId);

    Task<MessageReaction?> GetAsync(
        int messageId,
        int userId,
        string emoji);

    Task<MessageReaction> AddAsync(
        MessageReaction reaction);

    Task RemoveAsync(
        MessageReaction reaction);
}