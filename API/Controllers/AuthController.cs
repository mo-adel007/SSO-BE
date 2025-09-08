using Microsoft.EntityFrameworkCore;
using SsoBackend.Infrastructure;
using System.IdentityModel.Tokens.Jwt;

namespace SsoBackend.API.Controllers;

public class AuthController
{
    public async Task<IResult> GetGoogleOAuthUrl(GoogleOAuthService googleService)
    {
        var authUrl = googleService.GetGoogleAuthUrl();
        return Results.Json(new { url = authUrl });
    }

    public async Task<IResult> GetPlexOAuthUrl(PlexOAuthService plexService)
    {
        var authUrl = plexService.GetPlexAuthUrl();
        return Results.Json(new { url = authUrl });
    }

    public async Task<IResult> GetCurrentUser(
        HttpContext context, 
        AppDbContext db, 
        JwtService jwtService)
    {
        // Extract and validate JWT token
        var token = ExtractBearerToken(context);
        if (string.IsNullOrEmpty(token))
            return Results.Unauthorized();

        try
        {
            var userEmail = GetEmailFromToken(token);
            if (string.IsNullOrEmpty(userEmail))
                return Results.Unauthorized();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Results.Unauthorized();

            return Results.Json(new 
            { 
                id = user.Id,
                name = user.Name,
                email = user.Email 
            });
        }
        catch
        {
            return Results.Unauthorized();
        }
    }

    public async Task<IResult> HandleGoogleCallback(
        HttpContext context,
        GoogleOAuthService googleService,
        AppDbContext db,
        JwtService jwtService)
    {
        // Get authorization code from Google
        var code = context.Request.Query["code"].ToString();
        if (string.IsNullOrEmpty(code))
        {
            // Redirect to frontend with error
            var errorUrl = "http://localhost:5173/?error=Missing authorization code";
            return Results.Redirect(errorUrl);
        }

        try
        {
            // Exchange code for user info
            var userInfo = await googleService.ExchangeCodeAsync(code);
            if (userInfo == null)
            {
                var errorUrl = "http://localhost:5173/?error=Google authentication failed";
                return Results.Redirect(errorUrl);
            }

            // Find or create user
            var user = await FindOrCreateUser(db, userInfo);
            
            // Generate JWT token
            var jwtToken = jwtService.GenerateToken(user);
            
            // Redirect to frontend auth callback with token only (as your component expects)
            var successUrl = $"http://localhost:5173/auth/callback?token={jwtToken}";
            return Results.Redirect(successUrl);
        }
        catch (Exception ex)
        {
            // Redirect to frontend with error
            var errorUrl = $"http://localhost:5173/?error={Uri.EscapeDataString(ex.Message)}";
            return Results.Redirect(errorUrl);
        }
    }

    public async Task<IResult> HandlePlexCallback(
        HttpContext context,
        PlexOAuthService plexService,
        AppDbContext db,
        JwtService jwtService)
    {
        // Get authorization code from Plex
        var code = context.Request.Query["code"].ToString();
        if (string.IsNullOrEmpty(code))
        {
            // Redirect to frontend with error
            var errorUrl = "http://localhost:5173/?error=Missing authorization code";
            return Results.Redirect(errorUrl);
        }

        try
        {
            // Exchange code for user info
            var userInfo = await plexService.ExchangeCodeAsync(code);
            if (userInfo == null)
            {
                var errorUrl = "http://localhost:5173/?error=Plex authentication failed";
                return Results.Redirect(errorUrl);
            }

            // Find or create user
            var user = await FindOrCreateUser(db, userInfo);
            
            // Generate JWT token
            var jwtToken = jwtService.GenerateToken(user);
            
            // Redirect to frontend auth callback with token only (as your component expects)
            var successUrl = $"http://localhost:5173/auth/callback?token={jwtToken}";
            return Results.Redirect(successUrl);
        }
        catch (Exception ex)
        {
            // Redirect to frontend with error
            var errorUrl = $"http://localhost:5173/?error={Uri.EscapeDataString(ex.Message)}";
            return Results.Redirect(errorUrl);
        }
    }

    public async Task<IResult> Logout(HttpContext context)
    {
        // For JWT-based auth, logout is typically handled client-side by removing the token        
        var token = ExtractBearerToken(context);
        if (string.IsNullOrEmpty(token))
            return Results.BadRequest("No token provided");        

        return Results.Json(new { 
            success = true,
            message = "Logout successful. Please remove the token from client storage." 
        });
    }

    // Helper Methods
    private static string? ExtractBearerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;
            
        return authHeader.Substring("Bearer ".Length).Trim();
    }

    private static string? GetEmailFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        
        return jwt.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?
            .Value;
    }

    private static async Task<SsoBackend.Domain.User> FindOrCreateUser(
        AppDbContext db, 
        GoogleUserInfo userInfo)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == userInfo.Email);
        
        if (existingUser != null)
            return existingUser;

        // Create new user
        var newUser = new SsoBackend.Domain.User 
        { 
            Email = userInfo.Email, 
            Name = userInfo.Name 
        };
        
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        
        return newUser;
    }

    // Overload for Plex user info
    private static async Task<SsoBackend.Domain.User> FindOrCreateUser(
        AppDbContext db, 
        PlexUserInfo userInfo)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == userInfo.Email);
        
        if (existingUser != null)
            return existingUser;

        // Create new user
        var newUser = new SsoBackend.Domain.User 
        { 
            Email = userInfo.Email, 
            Name = userInfo.Name 
        };
        
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        
        return newUser;
    }
}
