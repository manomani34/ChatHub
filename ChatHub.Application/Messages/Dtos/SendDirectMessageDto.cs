namespace ChatHub.Application.Messages.Dtos;

public class SendDirectMessageDto
{
    public int ConversationId { get; set; }

    public int? ParentMessageId { get; set; }

    public string Content { get; set; } = string.Empty;
}