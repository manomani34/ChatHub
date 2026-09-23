using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IMessageRepository
{
    Task<List<Message>> GetByChannelIdAsync(int channelId);

    Task<Message> CreateAsync(Message message);
    Task<Message?> GetByIdAsync(int messageId);
    Task<Message> UpdateAsync(Message message);
}