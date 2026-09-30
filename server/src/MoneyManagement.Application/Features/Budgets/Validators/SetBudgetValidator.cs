using FluentValidation;
using MoneyManagement.Application.Features.Budgets.Requests;

namespace MoneyManagement.Application.Features.Budgets.Validators;

public class SetBudgetValidator : AbstractValidator<SetBudgetRequest>
{
    public SetBudgetValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("Budget can't be negative.");
        RuleFor(x => x.Month.Day).Equal(1).WithMessage("Month must be the first day of a month.");
    }
}
