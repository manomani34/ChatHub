using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class Message : BaseEntity
{
    public int SenderId { get; set; }

    public int? ChannelId { get; set; }

    public int? ConversationId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }

    public int? ParentMessageId { get; set; }

    public User Sender { get; set; } = null!;
    public Channel? Channel { get; set; }
    public Conversation? Conversation { get; set; }

    public Message? ParentMessage { get; set; }
    public ICollection<Message> Replies { get; set; }
        = new List<Message>();
}