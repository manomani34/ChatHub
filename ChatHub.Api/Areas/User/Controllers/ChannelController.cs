using System.Security.Claims;

using ChatHub.Application.Channels;
using ChatHub.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/channels")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly IWorkspaceMembershipRepository
        _membershipRepository;

    public ChannelController(
        IChannelService channelService,
        IWorkspaceMembershipRepository membershipRepository)
    {
        _channelService =
            channelService;

        _membershipRepository =
            membershipRepository;
    }

    [HttpGet("workspace/{workspaceId:int}")]
    public async Task<IActionResult> GetByWorkspace(
        int workspaceId)
    {
        if (workspaceId <= 0)
        {
            return BadRequest(
                "Invalid workspace id.");
        }

        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var isMember =
            await _membershipRepository.IsMemberAsync(
                workspaceId,
                userId.Value);

        if (!isMember)
        {
            return Forbid();
        }

        var channels =
            await _channelService
                .GetByWorkspaceIdAsync(
                    workspaceId);

        return Ok(channels);
    }

    private int? GetCurrentUserId()
    {
        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (
            claim is null ||
            !int.TryParse(
                claim.Value,
                out var userId) ||
            userId <= 0)
        {
            return null;
        }

        return userId;
    }
}