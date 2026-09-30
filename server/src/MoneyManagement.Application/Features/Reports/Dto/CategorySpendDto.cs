namespace MoneyManagement.Application.Features.Reports.Dto;

/// <summary>Spend in one category (a top-level one includes its sub-categories) and its share.</summary>
public class CategorySpendDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public long Spent { get; set; }
    public decimal SharePercent { get; set; }

    /// <summary>True when sub-categories had spend in the range, so the row can expand.</summary>
    public bool HasSubcategories { get; set; }
}
