using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Auth.Interfaces;

namespace MoneyManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string PkceCookieName = "finora_oauth_pkce";
    public const string RefreshCookieName = "finora_refresh_token";

    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;
    private readonly AuthOptions _authOptions;

    public AuthController(IAuthService authService, ICurrentUserService currentUser, IOptions<AuthOptions> authOptions)
    {
        _authService = authService;
        _currentUser = currentUser;
        _authOptions = authOptions.Value;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var request = _authService.BuildGoogleAuthorizationRequest();

        Response.Cookies.Append(PkceCookieName, $"{request.State}.{request.CodeVerifier}", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10),
        });

        return Redirect(request.AuthorizationUrl);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error, CancellationToken cancellationToken)
    {
        var loginErrorUrl = $"{_authOptions.FrontendBaseUrl}{_authOptions.LoginErrorPath}?error=denied";

        var pkceCookie = Request.Cookies[PkceCookieName];
        Response.Cookies.Delete(PkceCookieName);

        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state) || string.IsNullOrEmpty(pkceCookie))
            return Redirect(loginErrorUrl);

        var parts = pkceCookie.Split('.', 2);
        if (parts.Length != 2 || parts[0] != state)
            return Redirect(loginErrorUrl);

        var result = await _authService.HandleGoogleCallbackAsync(code, parts[1], cancellationToken);
        if (!result.IsSuccess)
            return Redirect(result.ErrorStatus == ErrorStatus.Forbidden ? $"{_authOptions.FrontendBaseUrl}{_authOptions.LoginErrorPath}?error=closed" : loginErrorUrl);

        SetRefreshTokenCookie(result.Data!);

        return Redirect($"{_authOptions.FrontendBaseUrl}{_authOptions.PostLoginPath}");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
        => (await _authService.GetMeAsync(_currentUser.UserId!.Value, cancellationToken)).ToActionResult();

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshCookieName] ?? string.Empty;
        var result = await _authService.RefreshAsync(rawToken, cancellationToken);

        if (!result.IsSuccess)
        {
            Response.Cookies.Delete(RefreshCookieName);
            return Result<AuthResultDto>.Failure(result.Message, result.ErrorStatus).ToActionResult();
        }

        SetRefreshTokenCookie(result.Data!.RefreshToken);

        return Result<AuthResultDto>.Success(result.Data.Result).ToActionResult();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(rawToken))
            await _authService.RevokeAsync(rawToken, cancellationToken);

        Response.Cookies.Delete(RefreshCookieName);

        return Result<object?>.Success(null, "Logged out.").ToActionResult();
    }

    private void SetRefreshTokenCookie(RawRefreshToken refreshToken)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken.Value, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = refreshToken.ExpiresAtUtc,
        });
    }
}
