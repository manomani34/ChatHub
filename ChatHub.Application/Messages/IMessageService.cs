using ChatHub.Application.Messages.Dtos;

namespace ChatHub.Application.Messages;

public interface IMessageService
{
    Task<List<MessageDto>> GetByChannelIdAsync(
        int channelId);

    Task<MessageDto?> GetByIdAsync(
        int messageId);

    Task<MessageDto> SendAsync(
        SendMessageDto request,
        int senderId);

    Task<MessageDto?> EditAsync(
        EditMessageDto request,
        int userId);

    Task<MessageDto?> DeleteAsync(
        int messageId,
        int userId);

    Task<List<MessageDto>> GetByConversationIdAsync(
    int conversationId);

    Task<MessageDto> SendToConversationAsync(
    SendDirectMessageDto request,
    int senderId);
}