using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _db;

    public MessageRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Message>> SearchAsync(
    string query,
    int? channelId = null,
    int? conversationId = null,
    int take = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Message>();

        if (take <= 0)
            take = 50;

        if (take > 100)
            take = 100;

        query = query.Trim();

        var messages = _db.Messages
            .AsNoTracking()
            .Include(x => x.Sender)
            .Where(x =>
                !x.IsDeleted &&
                EF.Functions.Like(x.Content, $"%{query}%"));

        if (channelId.HasValue)
        {
            messages = messages.Where(x =>
                x.ChannelId == channelId.Value);
        }

        if (conversationId.HasValue)
        {
            messages = messages.Where(x =>
                x.ConversationId == conversationId.Value);
        }

        return await messages
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Message?> GetLastMessageByConversationIdAsync(
    int conversationId)
    {
        if (conversationId <= 0)
            return null;

        return await _db.Messages
            .AsNoTracking()
            .Include(x => x.Sender)
            .Where(x =>
                x.ConversationId == conversationId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Message>> GetByConversationIdAsync(
    int conversationId)
    {
        if (conversationId <= 0)
            return new List<Message>();

        return await _db.Messages
            .Include(x => x.Sender)
            .Where(x =>
                x.ConversationId == conversationId &&
                !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Message>> GetByChannelIdAsync(int channelId)
    {
        return await _db.Messages
            .AsNoTracking()
            .Include(x => x.Sender)
            .Where(x =>
                x.ChannelId == channelId &&
                !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Message> CreateAsync(Message message)
    {
        _db.Messages.Add(message);

        await _db.SaveChangesAsync();

        return await _db.Messages
            .Include(x => x.Sender)
            .FirstAsync(x => x.Id == message.Id);
    }
    public async Task<Message?> GetByIdAsync(int messageId)
    {
        return await _db.Messages
            .Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId);
    }

    public async Task<Message> UpdateAsync(Message message)
    {
        _db.Messages.Update(message);

        await _db.SaveChangesAsync();

        return await _db.Messages
            .Include(x => x.Sender)
            .FirstAsync(x => x.Id == message.Id);
    }
}