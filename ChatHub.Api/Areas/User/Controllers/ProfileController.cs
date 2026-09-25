using ChatHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.User.Controllers;

[Area("User")]
[ApiController]
[Authorize]
[Route("api/user/profile")]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public ProfileController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile([FromQuery] string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest();

        var user =
            await _userRepository.GetByUserNameAsync(userName);

        if (user is null)
            return NotFound();

        return Ok(new
        {
            userId = user.Id,
            userName = user.UserName,
            displayName = user.DisplayName,
            email = user.Email
        });
    }
}