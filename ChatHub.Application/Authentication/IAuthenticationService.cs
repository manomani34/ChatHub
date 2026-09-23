using ChatHub.Application.Authentication.Dtos;

namespace ChatHub.Application.Authentication;

public interface IAuthenticationService
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request);
}