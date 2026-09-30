namespace MoneyManagement.Application.Common;

/// <summary>An inclusive calendar-date range.</summary>
public readonly record struct DateRange(DateOnly From, DateOnly To)
{
    public int Days => To.DayNumber - From.DayNumber + 1;
}

/// <summary>
/// The one place that turns "N months" or a custom from/to into dates. "N months" means the current
/// calendar month plus the previous N-1 (docs/project-overview.md §7, decision 6), not a rolling window.
/// </summary>
public static class DateRangeMath
{
    public const int MaxMonths = 24;
    public const int MaxCustomDays = 5 * 366;

    public static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    public static DateOnly EndOfMonth(DateOnly date) => StartOfMonth(date).AddMonths(1).AddDays(-1);

    /// <summary>The current calendar month and the <paramref name="months"/>-1 before it.</summary>
    public static DateRange LastMonths(int months, DateOnly today)
        => new(StartOfMonth(today).AddMonths(-(months - 1)), EndOfMonth(today));

    /// <summary>
    /// The immediately preceding period of equal length: the previous N calendar months for a named range,
    /// the same number of days for a custom one.
    /// </summary>
    public static DateRange PriorPeriod(DateRange range, int? months)
        => months is int n
            ? new(range.From.AddMonths(-n), range.From.AddDays(-1))
            : new(range.From.AddDays(-range.Days), range.From.AddDays(-1));

    /// <summary>First day of every calendar month the range touches, oldest first.</summary>
    public static IEnumerable<DateOnly> MonthsIn(DateRange range)
    {
        for (var m = StartOfMonth(range.From); m <= range.To; m = m.AddMonths(1))
            yield return m;
    }

    /// <summary>
    /// Resolves a named range (<paramref name="months"/>) or an explicit from/to. Returns the range, or an
    /// error message when neither shape is valid.
    /// </summary>
    public static (DateRange Range, string? Error) Resolve(int? months, DateOnly? from, DateOnly? to, DateOnly today)
    {
        if (from is not null || to is not null)
        {
            if (from is null || to is null)
                return (default, "Both 'from' and 'to' are required for a custom range.");
            if (from > to)
                return (default, "'from' must be on or before 'to'.");
            var custom = new DateRange(from.Value, to.Value);
            return custom.Days > MaxCustomDays ? (default, "A custom range can span at most 5 years.") : (custom, null);
        }

        var n = months ?? 6;
        return n is < 1 or > MaxMonths
            ? (default, $"Months must be between 1 and {MaxMonths}.")
            : (LastMonths(n, today), null);
    }
}
