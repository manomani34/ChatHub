using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Workspaces.Dtos;

namespace ChatHub.Application.Workspaces;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _repository;

    public WorkspaceService(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WorkspaceDto>> GetAllAsync()
    {
        var workspaces = await _repository.GetActiveAsync();

        return workspaces
            .Select(x => new WorkspaceDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                LogoUrl = x.LogoUrl,
                IsActive = x.IsActive
            })
            .ToList();
    }
}