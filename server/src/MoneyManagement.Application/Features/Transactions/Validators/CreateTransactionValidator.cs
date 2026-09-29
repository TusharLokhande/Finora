using FluentValidation;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Transactions.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);

        RuleFor(x => x.ToAccountId).NotNull().When(x => x.Type == TransactionType.Transfer)
            .WithMessage("A transfer needs a destination account.");
        RuleFor(x => x.CategoryId).Null().When(x => x.Type == TransactionType.Transfer)
            .WithMessage("Transfers cannot have a category.");

        RuleFor(x => x.ToAccountId).Null().When(x => x.Type != TransactionType.Transfer)
            .WithMessage("Only transfers can have a destination account.");
        RuleFor(x => x.CategoryId).NotNull().When(x => x.Type != TransactionType.Transfer)
            .WithMessage("A category is required.");
    }
}
