using SsoBackend.API.Controllers;
using SsoBackend.Infrastructure;

namespace SsoBackend.API.Routes;

public static class GoogleAuthRoutes
{
    public static void MapGoogleAuthRoutes(this WebApplication app)
    {
        app.MapGet("/auth/google/url", (GoogleOAuthService googleService) =>
        {
            var controller = new GoogleAuthController();
            return controller.GetGoogleOAuthUrl(googleService);
        });

        app.MapGet("/auth/google/callback", (HttpContext context, GoogleOAuthService googleService, AppDbContext db, JwtService jwtService) =>
        {
            var controller = new GoogleAuthController();
            return controller.HandleGoogleCallback(context, googleService, db, jwtService);
        });
    }
}
