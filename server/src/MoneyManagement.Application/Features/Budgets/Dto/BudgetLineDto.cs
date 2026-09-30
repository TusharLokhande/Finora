namespace MoneyManagement.Application.Features.Budgets.Dto;

/// <summary>One expense category's budget and spend for a month. Amounts in minor units.</summary>
public class BudgetLineDto
{
    public Guid CategoryId { get; set; }

    /// <summary>Set for a sub-category (which only appears when it has its own budget).</summary>
    public Guid? ParentId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string? ParentCategoryName { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool Active { get; set; }

    /// <summary>Null when no budget is set for this month.</summary>
    public long? Budgeted { get; set; }

    /// <summary>Spending in the category; a top-level category includes its sub-categories.</summary>
    public long Spent { get; set; }

    /// <summary>Null when unbudgeted or budgeted at zero.</summary>
    public decimal? PercentUsed { get; set; }

    public BudgetStatus Status { get; set; }
}
