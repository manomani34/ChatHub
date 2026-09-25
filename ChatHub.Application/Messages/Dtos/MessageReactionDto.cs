namespace ChatHub.Application.Messages.Dtos;

public class MessageReactionDto
{
    public string Emoji { get; set; } = string.Empty;

    public int Count { get; set; }

    public bool ReactedByCurrentUser { get; set; }
}