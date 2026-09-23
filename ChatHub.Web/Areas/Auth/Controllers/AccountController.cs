using System.Security.Claims;
using ChatHub.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ChatHub.Web.Areas.Auth.Controllers;

[Area("Auth")]
public class AccountController : Controller
{
    private readonly ChatHubApiClient _apiClient;

    public AccountController(
        ChatHubApiClient apiClient)
    {
        _apiClient = apiClient;
    }


    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Home",
                new { area = "User" });
        }

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string userName,
        string password,
        bool rememberMe = false)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            ModelState.AddModelError(
                "userName",
                "نام کاربری الزامی است.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "password",
                "رمز عبور الزامی است.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }


        var result =
            await _apiClient.LoginAsync(
                userName.Trim(),
                password);


        if (result is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "نام کاربری یا رمز عبور صحیح نیست.");

            return View();
        }


        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    result.UserId.ToString()),

                new(
                    ClaimTypes.Name,
                    result.UserName),

                new(
                    ClaimTypes.GivenName,
                    result.DisplayName)
            };


        var identity =
            new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme);


        var authenticationProperties =
            new AuthenticationProperties
            {
                IsPersistent =
                    rememberMe,

                ExpiresUtc =
                    result.ExpiresAt.ToUniversalTime(),

                AllowRefresh =
                    true
            };


        /*
         * JWT را داخل Authentication Cookie نگه می‌داریم.
         * در مرحله SignalR همین Token را از Cookie برمی‌داریم.
         */
        authenticationProperties.StoreTokens(
            new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = result.AccessToken
                }
            });


        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authenticationProperties);


        return RedirectToAction(
            "Index",
            "Home",
            new { area = "User" });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            nameof(Login));
    }


    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}