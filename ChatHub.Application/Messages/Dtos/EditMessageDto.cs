namespace ChatHub.Application.Messages.Dtos;

public class EditMessageDto
{
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
}