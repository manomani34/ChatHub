using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class Conversation : BaseEntity
{
    public bool IsGroup { get; set; }

    public string? Name { get; set; }

    public ICollection<ConversationMember> Members { get; set; }
        = new List<ConversationMember>();

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();
}