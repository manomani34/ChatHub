namespace ChatHub.Application.Workspaces.Dtos;

public class WorkspaceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; }
}