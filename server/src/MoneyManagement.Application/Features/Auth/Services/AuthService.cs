using Microsoft.Extensions.Logging;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Access.Interfaces;
using MoneyManagement.Application.Features.Auth.Interfaces;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IGoogleOAuthClient _googleOAuthClient;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingRepository _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IGoogleOAuthClient googleOAuthClient,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IAppSettingRepository settings,
        ILogger<AuthService> logger)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _googleOAuthClient = googleOAuthClient;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _settings = settings;
        _logger = logger;
    }

    public GoogleAuthorizationRequest BuildGoogleAuthorizationRequest()
    {
        var challenge = _googleOAuthClient.CreatePkceChallenge();
        var url = _googleOAuthClient.BuildAuthorizationUrl(challenge.State, challenge.CodeChallenge);
        return new GoogleAuthorizationRequest(url, challenge.State, challenge.CodeVerifier);
    }

    public async Task<Result<RawRefreshToken>> HandleGoogleCallbackAsync(string code, string codeVerifier, CancellationToken cancellationToken = default)
    {
        var tokenResult = await _googleOAuthClient.ExchangeCodeAsync(code, codeVerifier, cancellationToken);
        var identity = await _googleOAuthClient.ValidateIdTokenAsync(tokenResult.IdToken, cancellationToken);

        var user = await _users.GetByEmailAsync(identity.Email, cancellationToken);
        var now = DateTime.UtcNow;

        if (user is null)
        {
            if (!await _settings.GetSignupsOpenAsync(cancellationToken))
            {
                _logger.LogWarning("Sign-up rejected: new requests are closed");
                return Result<RawRefreshToken>.Failure("We're not accepting new requests right now.", ErrorStatus.Forbidden);
            }

            user = new User
            {
                Name = identity.Name,
                Email = identity.Email,
                Status = UserStatus.Pending,
                EmailVerifiedAtUtc = now,
                LastLoginAtUtc = now,
            };
            await _users.AddAsync(user, cancellationToken);
            _logger.LogInformation("New access request created for user {UserId}", user.Id);
        }
        else
        {
            // Heal rows created while Google sent no name (Name fell back to the email).
            if (user.Name == user.Email && identity.Name != identity.Email)
                user.Name = identity.Name;
            user.EmailVerifiedAtUtc ??= now;
            user.LastLoginAtUtc = now;
            _users.Update(user);
        }

        var refreshToken = _tokenService.GenerateRefreshToken();
        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshToken.Hash,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} signed in via Google (status {Status})", user.Id, user.Status);

        return Result<RawRefreshToken>.Success(refreshToken);
    }

    public async Task<Result<AuthUserDto>> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? Result<AuthUserDto>.Failure("Not signed in.", ErrorStatus.UnAuthorized)
            : Result<AuthUserDto>.Success(ToDto(user));
    }

    public async Task<Result<RefreshedSession>> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
            return Result<RefreshedSession>.Failure("Not signed in.", ErrorStatus.UnAuthorized);

        var hash = _tokenService.HashRefreshToken(rawRefreshToken);
        var existing = await _refreshTokens.GetByTokenHashAsync(hash, cancellationToken);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            // A revoked token being replayed is the signal worth alerting on.
            _logger.LogWarning("Refresh rejected: token {Reason}", existing is null ? "unknown" : existing.RevokedAtUtc is not null ? "already revoked (possible reuse)" : "expired");
            return Result<RefreshedSession>.Failure("Session expired. Please sign in again.", ErrorStatus.UnAuthorized);
        }

        var user = await _users.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshedSession>.Failure("Session expired. Please sign in again.", ErrorStatus.UnAuthorized);

        existing.RevokedAtUtc = DateTime.UtcNow;
        _refreshTokens.Update(existing);

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshToken.Hash,
            ExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
        }, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Name);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = new AuthResultDto(accessToken.Value, accessToken.ExpiresInSeconds, ToDto(user));

        return Result<RefreshedSession>.Success(new RefreshedSession(resultDto, newRefreshToken));
    }

    public async Task RevokeAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
            return;

        var hash = _tokenService.HashRefreshToken(rawRefreshToken);
        var existing = await _refreshTokens.GetByTokenHashAsync(hash, cancellationToken);
        if (existing is null || existing.RevokedAtUtc is not null)
            return;

        existing.RevokedAtUtc = DateTime.UtcNow;
        _refreshTokens.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} signed out", existing.UserId);
    }

    private static AuthUserDto ToDto(User user)
        => new(user.Id, user.Name, user.Email, user.Role, user.Status, user.RejectionReason);
}
