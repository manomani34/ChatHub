using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly AppDbContext _db;

    public ChannelRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Channel>> GetByWorkspaceIdAsync(int workspaceId)
    {
        return await _db.Channels
            .AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
    }
}