using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SsoBackend.Infrastructure;

public class PlexOAuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private string? _authorizationEndpoint;
    private string? _tokenEndpoint;
    private string? _userinfoEndpoint;
    private string? _revocationEndpoint;
    private string? _introspectionEndpoint;

    public PlexOAuthService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private async Task EnsureEndpointsAsync()
    {
        if (_authorizationEndpoint != null && _tokenEndpoint != null && _revocationEndpoint != null)
            return;

        var discoveryUrl = "https://accounts.plex.com/.well-known/openid-configuration";

        var doc = await _http.GetFromJsonAsync<JsonElement>(discoveryUrl);

        _authorizationEndpoint = doc.GetProperty("authorization_endpoint").GetString();
        _tokenEndpoint = doc.GetProperty("token_endpoint").GetString();
        _userinfoEndpoint = doc.GetProperty("userinfo_endpoint").GetString();
        _revocationEndpoint = doc.GetProperty("revocation_endpoint").GetString();
        _introspectionEndpoint = doc.GetProperty("introspection_endpoint").GetString();
    }

    public async Task<string> GetAuthorizationUrlAsync()
    {
        await EnsureEndpointsAsync();

        var clientId = _config["PLEX_CLIENT_ID"];
        var redirectUri = _config["PLEX_REDIRECT_URI"];
        var scope = "openid profile email";

        return $"{_authorizationEndpoint}?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scope)}";
    }

    public async Task<(string accessToken, string idToken)> ExchangeCodeForTokenAsync(string code)
    {
        await EnsureEndpointsAsync();

        var clientId = _config["PLEX_CLIENT_ID"];
        var clientSecret = _config["PLEX_CLIENT_SECRET"];
        var redirectUri = _config["PLEX_REDIRECT_URI"];

        var body = new FormUrlEncodedContent(
            new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
            }
        );

        var response = await _http.PostAsync(_tokenEndpoint, body);
        if (response == null)
        {
            throw new InvalidOperationException("No response from token endpoint");
        }
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Token endpoint returned {(int)response.StatusCode}: {raw}"
            );
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var accessToken = json.GetProperty("access_token").GetString();
        var idToken = json.GetProperty("id_token").GetString();

        return (accessToken!, idToken!);
    }

    public async Task<SsoBackend.Infrastructure.Models.PlexUserInfo> GetUserInfoAsync(
        string accessToken
    )
    {
        await EnsureEndpointsAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, _userinfoEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        string? email = json.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
        string? preferred = json.TryGetProperty("preferred_username", out var prefEl)
            ? prefEl.GetString()
            : null;
        string? name = json.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
        string? sub = json.TryGetProperty("sub", out var subEl) ? subEl.GetString() : null;

        return new SsoBackend.Infrastructure.Models.PlexUserInfo
        {
            Email = email ?? string.Empty,
            Name = preferred ?? name ?? email ?? string.Empty,
            Sub = sub ?? string.Empty,
        };
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        await EnsureEndpointsAsync();

        var clientId = _config["PLEX_CLIENT_ID"];
        var clientSecret = _config["PLEX_CLIENT_SECRET"];

        using var client = new HttpClient();
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(byteArray)
            );

        var content = new FormUrlEncodedContent(
            new[] { new KeyValuePair<string, string>("token", token) }
        );

        var response = await client.PostAsync(_revocationEndpoint, content);

        return response.IsSuccessStatusCode;
    }
}
