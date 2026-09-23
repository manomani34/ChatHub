using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);

    DateTime GetExpiration();
}