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