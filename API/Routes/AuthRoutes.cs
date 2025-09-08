using SsoBackend.API.Controllers;

namespace SsoBackend.API.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        // Get Google OAuth URL for frontend
        app.MapGet("/auth/google/url", (GoogleOAuthService googleService) =>
        {
            var authController = new AuthController();
            return authController.GetGoogleOAuthUrl(googleService);
        });
        
        // Get Plex OAuth URL for frontend
        app.MapGet("/auth/plex/url", (PlexOAuthService plexService) =>
        {
            var authController = new AuthController();
            return authController.GetPlexOAuthUrl(plexService);
        });
        
        // Get current authenticated user info
        app.MapGet("/auth/me", (HttpContext context, AppDbContext db, JwtService jwtService) =>
        {
            var authController = new AuthController();
            return authController.GetCurrentUser(context, db, jwtService);
        });
        
        // Handle Google OAuth callback
        app.MapGet("/auth/google/callback", (HttpContext context, GoogleOAuthService googleService, AppDbContext db, JwtService jwtService) =>
        {
            var authController = new AuthController();
            return authController.HandleGoogleCallback(context, googleService, db, jwtService);
        });
        
        // Handle Plex OAuth callback
        app.MapGet("/auth/plex/callback", (HttpContext context, PlexOAuthService plexService, AppDbContext db, JwtService jwtService) =>
        {
            var authController = new AuthController();
            return authController.HandlePlexCallback(context, plexService, db, jwtService);
        });
        
        // Logout endpoint
        app.MapPost("/auth/logout", (HttpContext context) =>
        {
            var authController = new AuthController();
            return authController.Logout(context);
        });
    }
}
