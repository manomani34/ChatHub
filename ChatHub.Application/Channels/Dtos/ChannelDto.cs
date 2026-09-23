namespace ChatHub.Application.Channels.Dtos;

public class ChannelDto
{
    public int Id { get; set; }

    public int WorkspaceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string? Description { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsActive { get; set; }
}