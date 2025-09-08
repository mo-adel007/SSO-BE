using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SsoBackend.Domain;

namespace SsoBackend.Infrastructure;

public class JwtService
{
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    public JwtService()
    {
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret_key_please_change";
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "sso-backend";
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name ?? ""),
            // Add role claims if needed
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
}
