using Microsoft.EntityFrameworkCore;
using SsoBackend.API.Controllers;
using SsoBackend.Infrastructure;

namespace SsoBackend.API.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        // JWT-based auth
        app.MapGet(
            "/auth/me",
            (HttpContext context, AppDbContext db, JwtService jwtService) =>
            {
                var controller = new UserController();
                return controller.GetCurrentUser(context, db, jwtService);
            }
        );

        // Session-based auth
        app.MapGet(
            "/auth/session/me",
            async (HttpContext context, AppDbContext db) =>
            {
                var email = context.Session.GetString("userEmail");
                if (string.IsNullOrEmpty(email))
                    return Results.Unauthorized();
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return Results.Unauthorized();
                return Results.Json(
                    new
                    {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email,
                    }
                );
            }
        );

        app.MapPost(
            "/auth/logout",
            async (HttpContext context, PlexOAuthService plexService) =>
            {
                var token = context.Session.GetString("access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    await plexService.RevokeTokenAsync(token);
                }
                context.Session.Clear();
                context.Response.Cookies.Delete(".AspNetCore.Session");
                return Results.Json(new { success = true, message = "Logout successful." });
            }
        );
    }
}
