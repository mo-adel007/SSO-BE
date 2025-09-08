using SsoBackend.Domain;

namespace SsoBackend.Infrastructure;

public class UserRepository : IUserRepository
{
    // TODO: Implement MSSQL logic here
    public Task<User?> GetByEmailAsync(string email) => Task.FromResult<User?>(null);
    public Task AddAsync(User user) => Task.CompletedTask;
    public Task AssignRoleAsync(Guid userId, string roleName) => Task.CompletedTask;
    public Task<List<User>> GetAllAsync() => Task.FromResult(new List<User>());
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task AssignRoleAsync(Guid userId, string roleName);
    Task<List<User>> GetAllAsync();
}
