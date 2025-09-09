using Microsoft.EntityFrameworkCore;
using SsoBackend.Infrastructure;
using System.IdentityModel.Tokens.Jwt;

namespace SsoBackend.API.Controllers;

public class GoogleAuthController
{
    public async Task<IResult> GetGoogleOAuthUrl(GoogleOAuthService googleService)
    {
        var authUrl = googleService.GetGoogleAuthUrl();
        return Results.Json(new { url = authUrl });
    }

    public async Task<IResult> HandleGoogleCallback(HttpContext context, GoogleOAuthService googleService, AppDbContext db, JwtService jwtService)
    {
        var code = context.Request.Query["code"].ToString();
        if (string.IsNullOrEmpty(code))
        {
            var errorUrl = "http://localhost:5173/?error=Missing authorization code";
            return Results.Redirect(errorUrl);
        }
        try
        {
            var userInfo = await googleService.ExchangeCodeAsync(code);
            if (userInfo == null)
            {
                var errorUrl = "http://localhost:5173/?error=Google authentication failed";
                return Results.Redirect(errorUrl);
            }
            var user = await AuthHelpers.FindOrCreateUser(db, userInfo);
            var jwtToken = jwtService.GenerateToken(user);
            var successUrl = $"http://localhost:5173/auth/callback?token={jwtToken}";
            return Results.Redirect(successUrl);
        }
        catch (Exception ex)
        {
            var errorUrl = $"http://localhost:5173/?error={Uri.EscapeDataString(ex.Message)}";
            return Results.Redirect(errorUrl);
        }
    }
}
