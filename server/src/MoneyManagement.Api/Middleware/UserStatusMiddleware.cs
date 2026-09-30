using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Auth.Interfaces;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Api.Middleware;

/// <summary>
/// Checks the signed-in user's live status/role on every /api call, so a suspension bites immediately
/// rather than when the JWT expires. /api/auth/* is exempt so the client can learn its status (/me) and sign out.
/// Also gates /api/access/* to admins.
/// </summary>
public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser, IUserRepository users)
    {
        var path = context.Request.Path;
        if (!currentUser.IsAuthenticated || !path.StartsWithSegments("/api") || path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }

        var user = currentUser.UserId is { } id ? await users.GetByIdAsync(id, context.RequestAborted) : null;

        if (user is null)
            await Reject(context, StatusCodes.Status401Unauthorized, "Session expired. Please sign in again.", ErrorStatus.UnAuthorized);
        else if (user.Status != UserStatus.Approved)
            await Reject(context, StatusCodes.Status403Forbidden, "Your account doesn't have access.", ErrorStatus.Forbidden);
        else if (path.StartsWithSegments("/api/access") && user.Role != UserRole.Admin)
            await Reject(context, StatusCodes.Status403Forbidden, "Admins only.", ErrorStatus.Forbidden);
        else
            await _next(context);
    }

    private static Task Reject(HttpContext context, int statusCode, string message, ErrorStatus status)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(ApiResponse<object>.FromResult(Result<object>.Failure(message, status)));
    }
}
