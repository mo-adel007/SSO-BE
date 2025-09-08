using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SsoBackend.Infrastructure;

public class PlexOAuthService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public PlexOAuthService(IConfiguration config)
    {
        _config = config;
        _http = new HttpClient();
    }

    public string GetPlexAuthUrl()
    {
        var clientId = _config["PLEX_CLIENT_ID"];
        var redirectUri = _config["PLEX_REDIRECT_URI"];
        var scope = "read"; // Adjust scope as needed
        return $"https://app.plex.tv/auth#?clientID={clientId}&forwardUrl={redirectUri}&context%5Bdevice%5D%5Bproduct%5D=YourApp";
    }

    public async Task<PlexUserInfo?> ExchangeCodeAsync(string code)
    {
        var clientId = _config["PLEX_CLIENT_ID"];
        var clientSecret = _config["PLEX_CLIENT_SECRET"];
        var tokenUrl = "https://plex.tv/api/v2/oauth/token";

        // Prepare Basic Auth header
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["code"] = code
        };

        var response = await _http.PostAsync(tokenUrl, new FormUrlEncodedContent(payload));
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Plex Token Response: " + json);

        var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
            return null;

        var accessToken = accessTokenElement.GetString();
        
        // Get user info using access token
        return await GetPlexUserInfo(accessToken);
    }

    private async Task<PlexUserInfo?> GetPlexUserInfo(string accessToken)
    {
        try
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("X-Plex-Token", accessToken);
            _http.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _http.GetAsync("https://plex.tv/api/v2/user");
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Plex User Info Response: " + json);

            var doc = JsonDocument.Parse(json);
            var email = doc.RootElement.GetProperty("email").GetString();
            var username = doc.RootElement.GetProperty("username").GetString();
            var title = doc.RootElement.TryGetProperty("title", out var titleElement) ? titleElement.GetString() : username;

            if (string.IsNullOrEmpty(email))
                return null;

            return new PlexUserInfo 
            { 
                Email = email, 
                Name = title ?? username ?? "Plex User" 
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting Plex user info: " + ex.Message);
            return null;
        }
    }
}

public class PlexUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
