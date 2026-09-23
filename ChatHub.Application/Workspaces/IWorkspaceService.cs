using ChatHub.Application.Workspaces.Dtos;

namespace ChatHub.Application.Workspaces;

public interface IWorkspaceService
{
    Task<List<WorkspaceDto>> GetAllAsync();
}