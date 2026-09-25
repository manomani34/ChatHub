using ChatHub.Application.Conversations.Dtos;

namespace ChatHub.Application.Conversations;

public interface IDirectMessageService
{
    Task<DirectConversationDto?> GetOrCreateConversationAsync(
        int currentUserId,
        string otherUserName);

    Task<List<DirectConversationDto>> GetDirectConversationsAsync(
    int currentUserId);
}