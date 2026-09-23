using ChatHub.Application.Channels.Dtos;
using ChatHub.Application.Common.Interfaces;

namespace ChatHub.Application.Channels;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _repository;

    public ChannelService(IChannelRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ChannelDto>> GetByWorkspaceIdAsync(int workspaceId)
    {
        var channels = await _repository
            .GetByWorkspaceIdAsync(workspaceId);

        return channels
            .Select(x => new ChannelDto
            {
                Id = x.Id,
                WorkspaceId = x.WorkspaceId,
                Name = x.Name,
                SortOrder = x.SortOrder,
                Description = x.Description,
                IsPrivate = x.IsPrivate,
                IsActive = x.IsActive
            })
            .ToList();
    }
}