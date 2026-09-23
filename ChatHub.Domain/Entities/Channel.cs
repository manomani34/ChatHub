using ChatHub.Domain.Common;

namespace ChatHub.Domain.Entities;

public class Channel : BaseEntity
{
    public int WorkspaceId { get; set; }

    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public string? Description { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsActive { get; set; } = true;

    public Workspace Workspace { get; set; } = null!;

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();
}