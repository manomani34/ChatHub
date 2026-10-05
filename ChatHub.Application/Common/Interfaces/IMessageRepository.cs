using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IMessageRepository
{
    Task<List<Message>> GetByChannelIdAsync(int channelId,
                                            int? beforeMessageId = null,
                                            int take = 50);

    Task<List<Message>> GetByConversationIdAsync(int conversationId,
                                                 int? beforeMessageId = null,
                                                 int take = 50);

    Task<Message> CreateAsync(Message message);
    Task<Message?> GetByIdAsync(int messageId);
    Task<Message> UpdateAsync(Message message);
    Task<Message?> GetLastMessageByConversationIdAsync(
    int conversationId);

    Task<List<Message>> SearchAsync(string query,
                                    int? channelId = null,
                                    int? conversationId = null,
                                    int take = 50);
}