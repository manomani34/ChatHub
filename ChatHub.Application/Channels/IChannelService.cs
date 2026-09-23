using ChatHub.Application.Channels.Dtos;

namespace ChatHub.Application.Channels;

public interface IChannelService
{
    Task<List<ChannelDto>> GetByWorkspaceIdAsync(int workspaceId);
}