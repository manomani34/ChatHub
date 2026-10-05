using ChatHub.Application.Common.Interfaces;

namespace ChatHub.Application.Conversations;

public class ConversationReadStateService
    : IConversationReadStateService
{
    private readonly IConversationReadStateRepository _repository;

    public ConversationReadStateService(
        IConversationReadStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> GetUnreadCountAsync(
        int userId,
        int conversationId)
    {
        if (userId <= 0 || conversationId <= 0)
            return 0;

        return await _repository.GetUnreadCountAsync(
            userId,
            conversationId);
    }

    public async Task MarkAsReadAsync(
        int userId,
        int conversationId,
        int? lastReadMessageId)
    {
        if (userId <= 0 || conversationId <= 0)
            return;

        await _repository.MarkAsReadAsync(
            userId,
            conversationId,
            lastReadMessageId);
    }
}