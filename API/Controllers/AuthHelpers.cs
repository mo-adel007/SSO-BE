using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SsoBackend.Domain;
using SsoBackend.Infrastructure;
using SsoBackend.Infrastructure.Models;

namespace SsoBackend.API.Controllers;

public static class AuthHelpers
{
    public static string? ExtractBearerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;
        return authHeader.Substring("Bearer ".Length).Trim();
    }

    public static string? GetEmailFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }

    public static async Task<User> FindOrCreateUser(AppDbContext db, GoogleUserInfo userInfo)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == userInfo.Email);
        if (existingUser != null)
            return existingUser;
        var newUser = new User { Email = userInfo.Email, Name = userInfo.Name };
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser;
    }

    public static async Task<User> FindOrCreateUser(AppDbContext db, PlexUserInfo userInfo)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == userInfo.Email);
        if (existingUser != null)
            return existingUser;
        var newUser = new User { Email = userInfo.Email, Name = userInfo.Name };
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser;
    }
}
