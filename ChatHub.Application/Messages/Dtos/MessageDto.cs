namespace ChatHub.Application.Messages.Dtos;

public class MessageDto
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string? SenderAvatarUrl { get; set; }

    public int? ChannelId { get; set; }

    public int? ConversationId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsEdited { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}