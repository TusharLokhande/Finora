using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;

namespace MoneyManagement.Infrastructure.Auth;

public class GoogleOAuthClient : IGoogleOAuthClient
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    private readonly HttpClient _httpClient;
    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleOAuthClient> _logger;

    public GoogleOAuthClient(HttpClient httpClient, IOptions<GoogleAuthOptions> options, ILogger<GoogleOAuthClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public GooglePkceChallenge CreatePkceChallenge()
    {
        var state = GenerateUrlSafeRandomString(32);
        var codeVerifier = GenerateUrlSafeRandomString(64);
        var codeChallenge = Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

        return new GooglePkceChallenge(state, codeVerifier, codeChallenge);
    }

    public string BuildAuthorizationUrl(string state, string codeChallenge)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "online",
            ["prompt"] = "select_account",
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
        };

        var query = string.Join('&', parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{AuthorizationEndpoint}?{query}";
    }

    public async Task<GoogleTokenResult> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken = default)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier,
        });

        using var response = await _httpClient.PostAsync(TokenEndpoint, form, cancellationToken);
        if (!response.IsSuccessStatusCode)
            _logger.LogError("Google token exchange failed with status {StatusCode}", (int)response.StatusCode);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Google token endpoint returned an empty response.");

        return new GoogleTokenResult(payload.IdToken, payload.AccessToken);
    }

    public async Task<GoogleIdentity> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId],
            });
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed");
            throw;
        }

        return new GoogleIdentity(payload.Subject, payload.Email, payload.Name ?? payload.Email);
    }

    private static string GenerateUrlSafeRandomString(int byteLength)
        => Base64UrlEncode(RandomNumberGenerator.GetBytes(byteLength));

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}
