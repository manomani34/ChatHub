using ChatHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Web.Areas.User.Controllers;

[Area("User")]
[Authorize]
public class ProfileController : Controller
{
    private readonly ChatHubApiClient _apiClient;

    public ProfileController(ChatHubApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(
        string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName =
                User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userName))
                return Unauthorized();
        }

        var result =
            await _apiClient.GetUserProfileAsync(userName);

        if (result is null)
            return NotFound();

        var model = new UserProfileViewModel
        {
            UserId = result.UserId.ToString(),
            UserName = result.UserName,
            DisplayName = result.DisplayName,
            Email = result.Email
        };

        return View(model);
    }
}


public class UserProfileViewModel
{
    public string? UserId { get; set; }

    public string UserName { get; set; } =
        string.Empty;

    public string DisplayName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;
}