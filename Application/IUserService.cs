using SsoBackend.Domain;

namespace SsoBackend.Application;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateUserAsync(string email, string name);
    Task AssignRoleAsync(Guid userId, string roleName);
    Task<List<User>> GetAllUsersAsync();
}
