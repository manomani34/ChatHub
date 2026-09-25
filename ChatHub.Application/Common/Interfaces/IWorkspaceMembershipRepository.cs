using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IWorkspaceMembershipRepository
{
    Task<bool> IsMemberAsync(
        int workspaceId,
        int userId);

    Task<bool> IsMemberOfChannelAsync(
        int channelId,
        int userId);

    Task<List<User>> GetMembersAsync(int workspaceId);
}