using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class MessageReaction : BaseEntity
{
    public int MessageId { get; set; }

    public int UserId { get; set; }

    public string Emoji { get; set; } = string.Empty;

    public Message Message { get; set; } = null!;

    public User User { get; set; } = null!;
}