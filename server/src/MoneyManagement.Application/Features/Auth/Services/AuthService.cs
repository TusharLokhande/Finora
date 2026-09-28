using Microsoft.Extensions.Options;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;
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
    private readonly AuthOptions _authOptions;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IGoogleOAuthClient googleOAuthClient,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<AuthOptions> authOptions)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _googleOAuthClient = googleOAuthClient;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _authOptions = authOptions.Value;
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

        var isAllowed = _authOptions.AllowedEmails
            .Any(email => string.Equals(email, identity.Email, StringComparison.OrdinalIgnoreCase));
        if (!isAllowed)
            return Result<RawRefreshToken>.Failure("This email is not on the invite list.", ErrorStatus.Forbidden);

        var user = await _users.GetByEmailAsync(identity.Email, cancellationToken);
        var now = DateTime.UtcNow;

        if (user is null)
        {
            user = new User
            {
                Name = identity.Name,
                Email = identity.Email,
                EmailVerifiedAtUtc = now,
                LastLoginAtUtc = now,
            };
            await _users.AddAsync(user, cancellationToken);
        }
        else
        {
            if (user.Status is UserStatus.Suspended or UserStatus.Rejected)
                return Result<RawRefreshToken>.Failure("This account can no longer sign in.", ErrorStatus.Forbidden);

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

        return Result<RawRefreshToken>.Success(refreshToken);
    }

    public async Task<Result<RefreshedSession>> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
            return Result<RefreshedSession>.Failure("Not signed in.", ErrorStatus.UnAuthorized);

        var hash = _tokenService.HashRefreshToken(rawRefreshToken);
        var existing = await _refreshTokens.GetByTokenHashAsync(hash, cancellationToken);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
            return Result<RefreshedSession>.Failure("Session expired. Please sign in again.", ErrorStatus.UnAuthorized);

        var user = await _users.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null || user.Status is UserStatus.Suspended or UserStatus.Rejected)
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

        var resultDto = new AuthResultDto(
            accessToken.Value,
            accessToken.ExpiresInSeconds,
            new AuthUserDto(user.Id, user.Name, user.Email));

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
    }
}
