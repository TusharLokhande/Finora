namespace MoneyManagement.Application.Common;

/// <summary>Shared percentage math (shares, changes). No I/O, easy to unit test.</summary>
public static class PercentMath
{
    /// <summary>part / whole as a percentage, 1 decimal. Null when whole is zero.</summary>
    public static decimal? Percent(long part, long whole)
        => whole == 0 ? null : Math.Round(part * 100m / whole, 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Change from previous to current as a percentage of |previous| (so a negative base, e.g. last
    /// month's net savings, still gives "up" for an improvement). Null when previous is zero.
    /// </summary>
    public static decimal? ChangePercent(long current, long previous)
        => previous == 0 ? null : Math.Round((current - previous) * 100m / Math.Abs(previous), 1, MidpointRounding.AwayFromZero);
}
