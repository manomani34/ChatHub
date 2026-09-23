using ChatHub.Application.Workspaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Route("api/user/workspaces")]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspaceController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var workspaces = await _workspaceService.GetAllAsync();

        return Ok(workspaces);
    }
}