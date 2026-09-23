using ChatHub.Application.Authentication;
using ChatHub.Application.Authentication.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Api.Areas.Auth.Controllers;

[Area("Auth")]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (request is null)
        {
            return BadRequest("Login request is required.");
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password is required.");
        }

        var result =
            await _authenticationService.LoginAsync(
                request);

        if (result is null)
        {
            return Unauthorized(
                new
                {
                    message =
                        "Username or password is invalid."
                });
        }

        return Ok(result);
    }
}