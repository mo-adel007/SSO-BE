using Microsoft.EntityFrameworkCore;
using SsoBackend.Infrastructure;

namespace SsoBackend.API.Controllers;

public class AdminController
{
    public async Task<IResult> GetAllUsers(HttpContext http, AppDbContext db)
    {
        // TODO: Require admin role authentication
        var users = await db.Users
            .Select(u => new { u.Id, u.Name, u.Email })
            .ToListAsync();
        
        return Results.Json(users);
    }

    public async Task<IResult> UpdateUserRole(int id, HttpContext http, AppDbContext db)
    {
        // TODO: Require admin role authentication
        // TODO: Implement role update logic
        // Example: update user in DB
        
        var user = await db.Users.FindAsync(id);
        if (user == null)
            return Results.NotFound($"User with ID {id} not found");
            
        // Role update logic would go here
        
        return Results.Ok($"User {id} role updated");
    }
}
