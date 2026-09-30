namespace MoneyManagement.Application.Features.Transactions.Dto;

/// <summary>Income and expense for one calendar month. Transfers are excluded.</summary>
public class MonthlyTotalDto
{
    /// <summary>First day of the month.</summary>
    public DateOnly Month { get; set; }
    public long Income { get; set; }
    public long Expense { get; set; }
    public long Net => Income - Expense;
}
