using FluentValidation;
using MoneyManagement.Application.Features.Accounts.Requests;

namespace MoneyManagement.Application.Features.Accounts.Validators;

public class ReorderAccountsValidator : AbstractValidator<ReorderAccountsRequest>
{
    public ReorderAccountsValidator()
    {
        RuleFor(x => x.AccountIds).NotEmpty();
        RuleForEach(x => x.AccountIds).NotEmpty();
    }
}
