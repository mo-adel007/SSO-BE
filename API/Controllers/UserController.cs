using Microsoft.EntityFrameworkCore;
using SsoBackend.API.Controllers;
using SsoBackend.Infrastructure;

namespace SsoBackend.API.Controllers;

public class UserController
{
    public async Task<IResult> GetCurrentUser(
        HttpContext context,
        AppDbContext db,
        JwtService jwtService
    )
    {
        var token = AuthHelpers.ExtractBearerToken(context);
        if (string.IsNullOrEmpty(token))
        {
            return Results.Unauthorized();
        }
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userEmail = AuthHelpers.GetEmailFromToken(token);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Results.Unauthorized();
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return Results.Unauthorized();
            }
            Console.WriteLine($"[Auth] User found: {user.Email} (ID: {user.Id})");
            return Results.Json(
                new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                }
            );
        }
        catch (Exception ex)
        {
            return Results.Unauthorized();
        }
    }
}
