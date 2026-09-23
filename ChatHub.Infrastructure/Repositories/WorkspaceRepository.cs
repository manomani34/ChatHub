using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _db;

    public WorkspaceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Workspace>> GetActiveAsync()
    {
        return await _db.Workspaces
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}