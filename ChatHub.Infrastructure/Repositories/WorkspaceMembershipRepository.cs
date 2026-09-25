using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class WorkspaceMembershipRepository
    : IWorkspaceMembershipRepository
{
    private readonly AppDbContext _db;

    public WorkspaceMembershipRepository(
        AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetMembersAsync(
    int workspaceId)
    {
        if (workspaceId <= 0)
            return new List<User>();

        return await _db.WorkspaceMembers
            .AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .Select(x => x.User)
            .OrderBy(x => x.DisplayName)
            .ToListAsync();
    }
    public async Task<bool> IsMemberAsync(
        int workspaceId,
        int userId)
    {
        if (workspaceId <= 0 || userId <= 0)
        {
            return false;
        }

        return await _db.WorkspaceMembers
            .AsNoTracking()
            .AnyAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.UserId == userId);
    }


    public async Task<bool> IsMemberOfChannelAsync(
        int channelId,
        int userId)
    {
        if (channelId <= 0 || userId <= 0)
        {
            return false;
        }

        return await (
            from channel in _db.Channels
            join member in _db.WorkspaceMembers
                on channel.WorkspaceId
                equals member.WorkspaceId
            where
                channel.Id == channelId &&
                channel.IsActive &&
                member.UserId == userId
            select member
        )
        .AnyAsync();
    }
}