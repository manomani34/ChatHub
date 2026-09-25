namespace ChatHub.Application.Messages.Dtos;

public class ToggleMessageReactionDto
{
    public int MessageId { get; set; }

    public string Emoji { get; set; } = string.Empty;
}