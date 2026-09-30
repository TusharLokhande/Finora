namespace MoneyManagement.Application.Features.Reports.Dto;

/// <summary>Ranked spend by category, largest first. Shares are of <see cref="Total"/>.</summary>
public class CategoryBreakdownDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public long Total { get; set; }
    public IReadOnlyList<CategorySpendDto> Categories { get; set; } = [];

    /// <summary>The rest beyond the top N; null when nothing was left over.</summary>
    public OtherSpendDto? Other { get; set; }
}
