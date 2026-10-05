using ChatHub.Application.Messages.Dtos;

namespace ChatHub.Application.Messages;

public interface IMessageService
{   
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

    Task<MessageDto> SendToConversationAsync(
    SendDirectMessageDto request,
    int senderId);

    Task<List<MessageDto>> SearchAsync(
    string query,
    int? channelId = null,
    int? conversationId = null,
    int take = 50);

    Task<List<MessageDto>> GetByChannelIdAsync(
    int channelId,
    int? beforeMessageId = null,
    int take = 50);

    Task<List<MessageDto>> GetByConversationIdAsync(
        int conversationId,
        int? beforeMessageId = null,
        int take = 50);
}