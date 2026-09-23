using ChatHub.Application.Channels;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Route("api/user/channels")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet("workspace/{workspaceId:int}")]
    public async Task<IActionResult> GetByWorkspace(int workspaceId)
    {
        var channels = await _channelService
            .GetByWorkspaceIdAsync(workspaceId);

        return Ok(channels);
    }
}