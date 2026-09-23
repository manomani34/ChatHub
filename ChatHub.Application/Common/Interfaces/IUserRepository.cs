using ChatHub.Domain.Entities;

namespace ChatHub.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string userName);
}