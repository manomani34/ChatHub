using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetActiveAsync();
}