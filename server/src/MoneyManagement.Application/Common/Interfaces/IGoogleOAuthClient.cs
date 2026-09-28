namespace MoneyManagement.Application.Common.Interfaces;

public record GooglePkceChallenge(string State, string CodeVerifier, string CodeChallenge);

public record GoogleTokenResult(string IdToken, string AccessToken);

public record GoogleIdentity(string Subject, string Email, string Name);

public interface IGoogleOAuthClient
{
    GooglePkceChallenge CreatePkceChallenge();

    string BuildAuthorizationUrl(string state, string codeChallenge);

    Task<GoogleTokenResult> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken = default);

    Task<GoogleIdentity> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}
