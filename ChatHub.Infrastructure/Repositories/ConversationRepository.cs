using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Conversations.Dtos;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _db;

    public ConversationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DirectConversationDto>>
    GetDirectConversationSummariesAsync(
        int userId)
    {
        if (userId <= 0)
            return new List<DirectConversationDto>();

        var query =
            from conversation in _db.Conversations.AsNoTracking()

            where
                !conversation.IsGroup &&
                conversation.Members.Any(
                    member =>
                        member.UserId == userId)

            let otherMember =
                conversation.Members
                    .Where(
                        member =>
                            member.UserId != userId)
                    .OrderBy(member => member.Id)
                    .Select(
                        member => new
                        {
                            member.UserId,
                            member.User.UserName,
                            member.User.DisplayName,
                            member.User.AvatarUrl
                        })
                    .FirstOrDefault()

            let lastMessage =
                conversation.Messages
                    .Where(
                        message =>
                            !message.IsDeleted)
                    .OrderByDescending(
                        message =>
                            message.CreatedAt)
                    .Select(
                        message => new
                        {
                            message.Content,
                            message.CreatedAt
                        })
                    .FirstOrDefault()

            let lastReadMessageId =
                _db.UserConversationReadStates
                    .Where(
                        state =>
                            state.UserId == userId &&
                            state.ConversationId ==
                                conversation.Id)
                    .Select(
                        state =>
                            state.LastReadMessageId)
                    .FirstOrDefault()

            select new DirectConversationDto
            {
                ConversationId =
                    conversation.Id,

                OtherUserId =
                    otherMember != null
                        ? otherMember.UserId
                        : 0,

                OtherUserName =
                    otherMember != null
                        ? otherMember.UserName
                        : string.Empty,

                OtherDisplayName =
                    otherMember != null
                        ? otherMember.DisplayName
                        : string.Empty,

                OtherAvatarUrl =
                    otherMember != null
                        ? otherMember.AvatarUrl
                        : null,

                LastMessageContent =
                    lastMessage != null
                        ? lastMessage.Content
                        : null,

                LastMessageAt =
                    lastMessage != null
                        ? lastMessage.CreatedAt
                        : null,

                UnreadCount =
                    conversation.Messages.Count(
                        message =>
                            !message.IsDeleted &&
                            message.SenderId != userId &&
                            (
                                !lastReadMessageId.HasValue ||
                                message.Id >
                                    lastReadMessageId.Value
                            ))
            };

        return await query
            .OrderByDescending(
                x => x.LastMessageAt)
            .ToListAsync();
    }

    public async Task<List<int>> GetMemberUserIdsAsync(
    int conversationId)
    {
        if (conversationId <= 0)
            return new List<int>();

        return await _db.ConversationMembers
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .Select(x => x.UserId)
            .ToListAsync();
    }

    public async Task<List<Conversation>>
    GetDirectConversationsAsync(int userId)
    {
        if (userId <= 0)
            return new List<Conversation>();

        return await _db.Conversations
            .AsNoTracking()
            .Include(x => x.Members)
                .ThenInclude(x => x.User)
            .Where(x =>
                !x.IsGroup &&
                x.Members.Any(m =>
                    m.UserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsConversationMemberAsync(
    int conversationId,
    int userId)
    {
        if (conversationId <= 0 || userId <= 0)
            return false;

        return await _db.ConversationMembers
            .AsNoTracking()
            .AnyAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId);
    }
    public async Task<bool> IsMemberAsync(
    int conversationId,
    int userId)
    {
        if (conversationId <= 0 || userId <= 0)
            return false;

        return await _db.ConversationMembers
            .AsNoTracking()
            .AnyAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId);
    }

    public async Task<Conversation?> GetDirectConversationAsync(
        int userId,
        int otherUserId)
    {
        if (userId <= 0 || otherUserId <= 0)
            return null;

        if (userId == otherUserId)
            return null;

        return await _db.Conversations
            .Include(x => x.Members)
                .ThenInclude(x => x.User)
            .Where(x =>
                !x.IsGroup &&
                x.Members.Any(m => m.UserId == userId) &&
                x.Members.Any(m => m.UserId == otherUserId))
            .FirstOrDefaultAsync();
    }

    public async Task<Conversation> CreateDirectConversationAsync(
        int userId,
        int otherUserId)
    {
        if (userId <= 0)
            throw new ArgumentException(
                "Invalid user id.",
                nameof(userId));

        if (otherUserId <= 0)
            throw new ArgumentException(
                "Invalid other user id.",
                nameof(otherUserId));

        if (userId == otherUserId)
            throw new ArgumentException(
                "A user cannot create a conversation with himself.");

        var existingConversation =
            await GetDirectConversationAsync(
                userId,
                otherUserId);

        if (existingConversation is not null)
            return existingConversation;

        var conversation = new Conversation
        {
            IsGroup = false,
            Name = null,
            CreatedAt = DateTime.UtcNow
        };

        conversation.Members.Add(
            new ConversationMember
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

        conversation.Members.Add(
            new ConversationMember
            {
                UserId = otherUserId,
                CreatedAt = DateTime.UtcNow
            });

        _db.Conversations.Add(conversation);

        await _db.SaveChangesAsync();

        return await GetDirectConversationAsync(
                   userId,
                   otherUserId)
               ?? conversation;
    }
}