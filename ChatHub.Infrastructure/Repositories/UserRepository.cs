using ChatHub.Application.Common.Interfaces;
using ChatHub.Domain.Entities;
using ChatHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUserNameAsync(
        string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        return await _db.Users
            .FirstOrDefaultAsync(x =>
                x.UserName == userName);
    }

    public async Task<User?> GetByIdAsync(
    int userId)
    {
        if (userId <= 0)
            return null;

        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == userId);
    }

    public async Task UpdateLastSeenAsync(
    int userId,
    DateTime lastSeenAt)
    {
        if (userId <= 0)
            return;

        var user =
            await _db.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == userId);

        if (user is null)
            return;

        user.LastSeenAt =
            lastSeenAt;

        user.UpdatedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}