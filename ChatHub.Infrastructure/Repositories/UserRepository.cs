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
}