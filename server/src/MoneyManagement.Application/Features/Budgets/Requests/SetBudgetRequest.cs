namespace MoneyManagement.Application.Features.Budgets.Requests;

/// <summary>Creates or replaces one category's budget for one month.</summary>
public class SetBudgetRequest
{
    public Guid CategoryId { get; set; }

    /// <summary>Always the first day of the month.</summary>
    public DateOnly Month { get; set; }

    /// <summary>Minor units, &gt;= 0.</summary>
    public long Amount { get; set; }
}
