using FloraLink.Domain.Entities;

namespace FloraLink.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> AddAsync(User user);
    Task<bool> ExistsAsync(string email);
}
