namespace ChatHub.Application.Conversations.Dtos;

public class DirectConversationDto
{
    public int ConversationId { get; set; }

    public int OtherUserId { get; set; }

    public string OtherUserName { get; set; } =
        string.Empty;

    public string OtherDisplayName { get; set; } =
        string.Empty;

    public string? OtherAvatarUrl { get; set; }

    public int UnreadCount { get; set; }

    public string? LastMessageContent { get; set; }

    public DateTime? LastMessageAt { get; set; }
}