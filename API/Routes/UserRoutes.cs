using SsoBackend.API.Controllers;
using SsoBackend.Infrastructure;

namespace SsoBackend.API.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        app.MapGet("/auth/me", (HttpContext context, AppDbContext db, JwtService jwtService) =>
        {
            var controller = new UserController();
            return controller.GetCurrentUser(context, db, jwtService);
        });

        app.MapPost("/auth/logout", (HttpContext context) =>
        {
            // For JWT, logout is client-side; just return success
            return Results.Json(new { success = true, message = "Logout successful. Please remove the token from client storage." });
        });
    }
}
