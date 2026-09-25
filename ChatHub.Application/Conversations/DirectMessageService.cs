using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Conversations.Dtos;

namespace ChatHub.Application.Conversations;

public class DirectMessageService : IDirectMessageService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;

    public DirectMessageService(
        IConversationRepository conversationRepository,
        IUserRepository userRepository)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
    }

    /* =========================================================
       Get Direct Conversations
       ========================================================= */

    public async Task<List<DirectConversationDto>>
        GetDirectConversationsAsync(
            int currentUserId)
    {
        if (currentUserId <= 0)
            return new List<DirectConversationDto>();

        return await _conversationRepository
            .GetDirectConversationSummariesAsync(
                currentUserId);
    }

    /* =========================================================
       Get Or Create Direct Conversation
       ========================================================= */

    public async Task<DirectConversationDto?>
        GetOrCreateConversationAsync(
            int currentUserId,
            string otherUserName)
    {
        if (currentUserId <= 0)
            return null;

        if (string.IsNullOrWhiteSpace(otherUserName))
            return null;

        var otherUser =
            await _userRepository
                .GetByUserNameAsync(
                    otherUserName.Trim());

        if (otherUser is null)
            return null;

        if (otherUser.Id == currentUserId)
            return null;

        var conversation =
            await _conversationRepository
                .CreateDirectConversationAsync(
                    currentUserId,
                    otherUser.Id);

        return new DirectConversationDto
        {
            ConversationId =
                conversation.Id,

            OtherUserId =
                otherUser.Id,

            OtherUserName =
                otherUser.UserName,

            OtherDisplayName =
                otherUser.DisplayName,

            OtherAvatarUrl =
                otherUser.AvatarUrl
        };
    }
}