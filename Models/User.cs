namespace SsoBackend.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Role> Roles { get; set; } = new();
}
