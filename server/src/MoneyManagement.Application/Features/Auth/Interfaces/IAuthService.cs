using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;

namespace MoneyManagement.Application.Features.Auth.Interfaces;

public record AuthUserDto(Guid Id, string Name, string Email);

public record AuthResultDto(string AccessToken, int ExpiresInSeconds, AuthUserDto User);

public record GoogleAuthorizationRequest(string AuthorizationUrl, string State, string CodeVerifier);

public record RefreshedSession(AuthResultDto Result, RawRefreshToken RefreshToken);

public interface IAuthService
{
    GoogleAuthorizationRequest BuildGoogleAuthorizationRequest();

    Task<Result<RawRefreshToken>> HandleGoogleCallbackAsync(string code, string codeVerifier, CancellationToken cancellationToken = default);

    Task<Result<RefreshedSession>> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
}
