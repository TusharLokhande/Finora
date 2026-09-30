namespace MoneyManagement.Application.Features.Reports.Requests;

/// <summary>
/// Either a named range (<see cref="Months"/>: the current calendar month plus the previous N-1) or an
/// explicit <see cref="From"/>/<see cref="To"/>. Defaults to the last 6 months. Resolved by DateRangeMath.
/// </summary>
public class ReportRangeRequest
{
    public int? Months { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
