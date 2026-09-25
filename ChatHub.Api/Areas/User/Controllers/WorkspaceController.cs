using System.Security.Claims;

using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Workspaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/workspaces")]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    private readonly IWorkspaceMembershipRepository
        _membershipRepository;

    public WorkspaceController(
        IWorkspaceService workspaceService,
        IWorkspaceMembershipRepository membershipRepository)
    {
        _workspaceService =
            workspaceService;

        _membershipRepository =
            membershipRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId =
            GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var workspaces =
            await _workspaceService.GetAllAsync();

        var accessibleWorkspaces =
            new List<object>();

        foreach (var workspace in workspaces)
        {
            var isMember =
                await _membershipRepository.IsMemberAsync(
                    workspace.Id,
                    userId.Value);

            if (isMember)
            {
                accessibleWorkspaces.Add(workspace);
            }
        }

        return Ok(accessibleWorkspaces);
    }

    [HttpGet("{workspaceId:int}/members")]
    public async Task<IActionResult> GetMembers(int workspaceId)
    {
        if (workspaceId <= 0)
            return BadRequest();

        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var isMember =
            await _membershipRepository.IsMemberAsync(
                workspaceId,
                userId.Value);

        if (!isMember)
            return NotFound();

        var members =
            await _membershipRepository.GetMembersAsync(
                workspaceId);

        var result = members.Select(x => new
        {
            userId = x.Id,
            userName = x.UserName,
            displayName = x.DisplayName,
            email = x.Email,
            avatarUrl = x.AvatarUrl,
            lastSeenAt = x.LastSeenAt,
            isActive = x.IsActive
        });

        return Ok(result);
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