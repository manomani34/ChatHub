using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class ConversationReadStateRepository
    : IConversationReadStateRepository
{
    private readonly AppDbContext _db;

    public ConversationReadStateRepository(
        AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetUnreadCountAsync(
    int userId,
    int conversationId)
    {
        if (userId <= 0 || conversationId <= 0)
            return 0;

        var state =
            await _db.UserConversationReadStates
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ConversationId == conversationId);

        var query =
            _db.Messages
                .AsNoTracking()
                .Where(x =>
                    x.ConversationId == conversationId &&
                    !x.IsDeleted &&
                    x.SenderId != userId);

        if (state?.LastReadMessageId is not null)
        {
            query = query.Where(x =>
                x.Id > state.LastReadMessageId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<UserConversationReadState?> GetAsync(
        int userId,
        int conversationId)
    {
        if (userId <= 0 || conversationId <= 0)
            return null;

        return await _db.UserConversationReadStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.ConversationId == conversationId);
    }

    public async Task<UserConversationReadState>
        GetOrCreateAsync(
            int userId,
            int conversationId)
    {
        if (userId <= 0)
            throw new ArgumentException(
                "Invalid user id.",
                nameof(userId));

        if (conversationId <= 0)
            throw new ArgumentException(
                "Invalid conversation id.",
                nameof(conversationId));

        var state =
            await _db.UserConversationReadStates
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ConversationId == conversationId);

        if (state is not null)
            return state;

        state = new UserConversationReadState
        {
            UserId = userId,
            ConversationId = conversationId,
            LastReadMessageId = null,
            LastReadAt = null,
            CreatedAt = DateTime.UtcNow
        };

        _db.UserConversationReadStates.Add(state);

        await _db.SaveChangesAsync();

        return state;
    }

    public async Task MarkAsReadAsync(
        int userId,
        int conversationId,
        int? lastReadMessageId)
    {
        var state =
            await GetOrCreateAsync(
                userId,
                conversationId);

        state.LastReadMessageId =
            lastReadMessageId;

        state.LastReadAt =
            DateTime.UtcNow;

        state.UpdatedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}