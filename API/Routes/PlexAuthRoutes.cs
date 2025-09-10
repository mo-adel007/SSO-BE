using SsoBackend.API.Controllers;
using SsoBackend.Infrastructure;

namespace SsoBackend.API.Routes;

public static class PlexAuthRoutes
{
    public static void MapPlexAuthRoutes(this WebApplication app)
    {
        app.MapGet(
            "/auth/plex/url",
            async (PlexOAuthService plexService) =>
            {
                var controller = new PlexAuthController();
                return await controller.GetPlexOAuthUrl(plexService);
            }
        );

        app.MapGet(
            "/auth/plex/callback",
            async (
                HttpContext context,
                PlexOAuthService plexService,
                AppDbContext db,
                JwtService jwtService
            ) =>
            {
                var controller = new PlexAuthController();
                return await controller.HandlePlexCallback(context, plexService, db, jwtService);
            }
        );
    }
}
