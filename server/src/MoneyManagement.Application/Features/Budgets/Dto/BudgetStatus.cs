namespace MoneyManagement.Application.Features.Budgets.Dto;

/// <summary>Visual state of a budget: normal below 80% used, warning from 80%, over from 100%.</summary>
public enum BudgetStatus
{
    Normal,
    Warning,
    Over
}
