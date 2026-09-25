namespace ChatHub.Application.Messages.Dtos;

public class SendMessageDto
{
    public int ChannelId { get; set; }

    public int? ParentMessageId { get; set; }

    public string Content { get; set; } = string.Empty;
}