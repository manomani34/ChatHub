namespace ChatHub.Application.Conversations;

public interface IConversationReadStateService
{
    Task<int> GetUnreadCountAsync(
        int userId,
        int conversationId);

    Task MarkAsReadAsync(
        int userId,
        int conversationId,
        int? lastReadMessageId);
}