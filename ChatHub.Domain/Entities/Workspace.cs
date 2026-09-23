using ChatHub.Domain.Common;
using System.Threading.Channels;

namespace ChatHub.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<WorkspaceMember> Members { get; set; }
        = new List<WorkspaceMember>();

    public ICollection<Channel> Channels { get; set; }
        = new List<Channel>();
}