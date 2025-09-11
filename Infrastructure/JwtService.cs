using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SsoBackend.Models;

namespace SsoBackend.Infrastructure;

public class JwtService
{
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;

    public JwtService()
    {
        _jwtSecret =
            Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret_key_please_change";
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "sso-backend";
    }

    // For cases where you already have a User entity (Google/Plex flows)
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name ?? ""),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // For flexibility (raw email + claims, if no DB user available)
    public string GenerateToken(string email, Dictionary<string, object>? extraClaims = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
        };

        if (extraClaims != null)
        {
            foreach (var kvp in extraClaims)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
