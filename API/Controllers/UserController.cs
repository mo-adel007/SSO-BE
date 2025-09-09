using Microsoft.EntityFrameworkCore;
using SsoBackend.Infrastructure;
using SsoBackend.API.Controllers;

namespace SsoBackend.API.Controllers;

public class UserController
{
    public async Task<IResult> GetCurrentUser(HttpContext context, AppDbContext db, JwtService jwtService)
    {
        var token = AuthHelpers.ExtractBearerToken(context);
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("[Auth] No bearer token found in request");
            return Results.Unauthorized();
        }
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Console.WriteLine($"[Auth] JWT Claims: {string.Join(", ", jwt.Claims.Select(c => c.Type + ":" + c.Value))}");
            var userEmail = AuthHelpers.GetEmailFromToken(token);
            Console.WriteLine($"[Auth] Extracted email from token: {userEmail}");
            if (string.IsNullOrEmpty(userEmail))
            {
                Console.WriteLine("[Auth] No email found in token");
                return Results.Unauthorized();
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                Console.WriteLine($"[Auth] No user found in DB for email: {userEmail}");
                return Results.Unauthorized();
            }
            Console.WriteLine($"[Auth] User found: {user.Email} (ID: {user.Id})");
            return Results.Json(new { id = user.Id, name = user.Name, email = user.Email });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auth] Exception in GetCurrentUser: {ex.Message}");
            return Results.Unauthorized();
        }
    }
}
