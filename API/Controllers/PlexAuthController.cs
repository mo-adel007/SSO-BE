using Microsoft.EntityFrameworkCore;
using SsoBackend.Infrastructure;
using SsoBackend.Infrastructure.Models;
using System.Text.Json;

namespace SsoBackend.API.Controllers;

public class PlexAuthController
{
    public async Task<IResult> GetPlexOAuthUrl(PlexOAuthService plexService)
    {
        var url = await plexService.GetAuthorizationUrlAsync();
        return Results.Json(new { url });
    }

    public async Task<IResult> HandlePlexCallback(HttpContext context, PlexOAuthService plexService, AppDbContext db, JwtService jwtService)
    {
        var code = context.Request.Query["code"].ToString();
        if (string.IsNullOrEmpty(code))
            return Results.Redirect("/login?error=missing_code");

        try
        {
            var (accessToken, idToken) = await plexService.ExchangeCodeForTokenAsync(code);
            var userInfo = await plexService.GetUserInfoAsync(accessToken);
            var user = await AuthHelpers.FindOrCreateUser(db, userInfo);
            // Set session
            context.Session.SetString("userEmail", user.Email);
            var jwt = jwtService.GenerateToken(user);
            var redirectUrl = $"http://localhost:5173/auth/callback?token={jwt}";
            return Results.Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            return Results.Redirect($"/login?error=true&message={Uri.EscapeDataString(ex.Message)}");
        }
    }
}
