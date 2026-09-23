using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IChannelRepository
{
    Task<List<Channel>> GetByWorkspaceIdAsync(int workspaceId);
}