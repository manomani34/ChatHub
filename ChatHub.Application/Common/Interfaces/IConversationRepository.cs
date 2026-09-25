using ChatHub.Application.Conversations.Dtos;
using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetDirectConversationAsync(
        int userId,
        int otherUserId);

    Task<Conversation> CreateDirectConversationAsync(
        int userId,
        int otherUserId);

    Task<bool> IsMemberAsync(
    int conversationId,
    int userId);

    Task<bool> IsConversationMemberAsync(
    int conversationId,
    int userId);

    Task<List<Conversation>> GetDirectConversationsAsync(
    int userId);

    Task<List<int>> GetMemberUserIdsAsync(
    int conversationId);

    Task<List<DirectConversationDto>>
    GetDirectConversationSummariesAsync(
        int userId);
}