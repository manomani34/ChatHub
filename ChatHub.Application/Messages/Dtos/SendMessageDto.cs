namespace ChatHub.Application.Messages.Dtos;

public class SendMessageDto
{
    public int SenderId { get; set; }

    public int ChannelId { get; set; }

    public string Content { get; set; } = string.Empty;
}