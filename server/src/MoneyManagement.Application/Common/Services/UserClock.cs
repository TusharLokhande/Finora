using MoneyManagement.Application.Features.Auth.Interfaces;

namespace MoneyManagement.Application.Common.Services;

/// <summary>Today's date in the user's timezone (docs: months are interpreted in the user's timezone).</summary>
public class UserClock
{
    private readonly IUserRepository _users;
    private readonly TimeProvider _time;

    public UserClock(IUserRepository users, TimeProvider time)
    {
        _users = users;
        _time = time;
    }

    public async Task<DateOnly> TodayAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        var zone = TimeZoneInfo.Utc;
        if (user?.Timezone is { } id && TimeZoneInfo.TryFindSystemTimeZoneById(id, out var found))
            zone = found;

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_time.GetUtcNow(), zone).DateTime);
    }
}
