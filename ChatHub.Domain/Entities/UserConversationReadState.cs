using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class UserConversationReadState : BaseEntity
{
    public int UserId { get; set; }

    public int ConversationId { get; set; }

    public int? LastReadMessageId { get; set; }

    public DateTime? LastReadAt { get; set; }

    public User User { get; set; } = null!;

    public Conversation Conversation { get; set; } = null!;
}