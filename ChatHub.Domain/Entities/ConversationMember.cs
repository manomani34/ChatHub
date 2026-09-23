using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class ConversationMember : BaseEntity
{
    public int ConversationId { get; set; }

    public int UserId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public User User { get; set; } = null!;
}