using ChatHub.Application.Messages.Dtos;

namespace ChatHub.Application.Messages;

public interface IMessageService
{
    Task<List<MessageDto>> GetByChannelIdAsync(int channelId);

    Task<MessageDto> SendAsync(SendMessageDto request);
    Task<MessageDto?> EditAsync(EditMessageDto request);
    Task<MessageDto?> DeleteAsync(int messageId,
                                  int userId);
}