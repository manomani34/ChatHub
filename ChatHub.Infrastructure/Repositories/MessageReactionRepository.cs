using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class MessageReactionRepository
    : IMessageReactionRepository
{
    private readonly AppDbContext _db;

    public MessageReactionRepository(
        AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MessageReaction>>
        GetByMessageIdAsync(
            int messageId)
    {
        if (messageId <= 0)
            return new List<MessageReaction>();

        return await _db.MessageReactions
            .AsNoTracking()
            .Where(x =>
                x.MessageId == messageId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<MessageReaction?> GetAsync(
        int messageId,
        int userId,
        string emoji)
    {
        if (
            messageId <= 0 ||
            userId <= 0 ||
            string.IsNullOrWhiteSpace(emoji))
        {
            return null;
        }

        return await _db.MessageReactions
            .FirstOrDefaultAsync(x =>
                x.MessageId == messageId &&
                x.UserId == userId &&
                x.Emoji == emoji);
    }

    public async Task<MessageReaction>
        AddAsync(
            MessageReaction reaction)
    {
        reaction.CreatedAt =
            DateTime.UtcNow;

        _db.MessageReactions.Add(
            reaction);

        await _db.SaveChangesAsync();

        return reaction;
    }

    public async Task RemoveAsync(
        MessageReaction reaction)
    {
        _db.MessageReactions.Remove(
            reaction);

        await _db.SaveChangesAsync();
    }
}