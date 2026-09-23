using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class WorkspaceMember : BaseEntity
{
    public int WorkspaceId { get; set; }

    public int UserId { get; set; }

    public bool IsOwner { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public User User { get; set; } = null!;
}