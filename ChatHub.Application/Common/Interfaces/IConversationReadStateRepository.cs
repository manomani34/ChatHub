using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IConversationReadStateRepository
{
    Task<UserConversationReadState?> GetAsync(
        int userId,
        int conversationId);

    Task<UserConversationReadState> GetOrCreateAsync(
        int userId,
        int conversationId);

    Task MarkAsReadAsync(
        int userId,
        int conversationId,
        int? lastReadMessageId);

    Task<int> GetUnreadCountAsync(
    int userId,
    int conversationId);
}