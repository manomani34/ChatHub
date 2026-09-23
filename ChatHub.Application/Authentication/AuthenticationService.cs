using ChatHub.Application.Authentication.Dtos;
using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChatHub.Application.Authentication;

public class AuthenticationService
    : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request)
    {
        if (request is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user =
            await _userRepository.GetByUserNameAsync(
                request.UserName.Trim());

        if (user is null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null;
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (
            verificationResult !=
            PasswordVerificationResult.Success &&
            verificationResult !=
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            return null;
        }

        var token =
            _jwtTokenService.GenerateToken(user);

        var expiresAt =
            _jwtTokenService.GetExpiration();

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName
        };
    }
}