using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChatHub.Api.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT SecretKey is not configured.");
        }

        if (_options.SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must be at least 32 characters long.");
        }
    }


    public string GenerateToken(User user)
    {
        var now = DateTime.UtcNow;

        var expiresAt =
            now.AddMinutes(
                _options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.UniqueName,
                user.UserName),

            new(
                ClaimTypes.Name,
                user.UserName),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.GivenName,
                user.DisplayName),

            new(
                ClaimTypes.Email,
                user.Email)
        };


        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.SecretKey));


        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);


        var token =
            new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);


        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }


    public DateTime GetExpiration()
    {
        return DateTime.UtcNow.AddMinutes(
            _options.ExpirationMinutes);
    }
}