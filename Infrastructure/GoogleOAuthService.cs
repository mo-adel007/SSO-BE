using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SsoBackend.Models;

namespace SsoBackend.Infrastructure;

public class GoogleOAuthService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public GoogleOAuthService(IConfiguration config)
    {
        _config = config;
        _http = new HttpClient();
    }

    public string GetGoogleAuthUrl()
    {
        var clientId = _config["CLIENT_ID"];
        var redirectUri = _config["REDIRECT_URI"];
        var scope = "openid email profile";
        return $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";
    }

    public async Task<GoogleUserInfo?> ExchangeCodeAsync(string code)
    {
        var clientId = _config["CLIENT_ID"];
        var clientSecret = _config["CLIENT_SECRET"];
        var redirectUri = _config["REDIRECT_URI"];
        var tokenUrl = "https://oauth2.googleapis.com/token";
        var payload = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!,
            ["redirect_uri"] = redirectUri!,
            ["grant_type"] = "authorization_code",
        };
        var response = await _http.PostAsync(tokenUrl, new FormUrlEncodedContent(payload));
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Google Token Response: " + json); // <-- Add this line
        var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("id_token", out var idTokenElement))
            return null;
        var idToken = idTokenElement.GetString();

        // Decode JWT to get user info
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(idToken);
        foreach (var claim in jwt.Claims)
        {
            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        if (string.IsNullOrEmpty(email))
            return null;

        return new GoogleUserInfo { Email = email, Name = name ?? "" };
    }
}
