using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastSeenAt { get; set; }

    public ICollection<WorkspaceMember> WorkspaceMembers { get; set; }
        = new List<WorkspaceMember>();

    public ICollection<ConversationMember> ConversationMembers { get; set; }
        = new List<ConversationMember>();

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();
}