using FluentValidation;
using MoneyManagement.Application.Features.Accounts.Requests;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Accounts.Validators;

public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a hex value like #F97316.")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.CreditLimit)
            .Null().WithMessage("Credit limit can only be set for a credit card.")
            .When(x => x.Type != AccountType.CreditCard);
        RuleFor(x => x.StatementDay)
            .Null().WithMessage("Statement day can only be set for a credit card.")
            .When(x => x.Type != AccountType.CreditCard);
        RuleFor(x => x.DueDay)
            .Null().WithMessage("Due day can only be set for a credit card.")
            .When(x => x.Type != AccountType.CreditCard);

        RuleFor(x => x.CreditLimit!.Value)
            .GreaterThan(0)
            .When(x => x.Type == AccountType.CreditCard && x.CreditLimit is not null);
        RuleFor(x => x.StatementDay!.Value)
            .InclusiveBetween((short)1, (short)28)
            .When(x => x.Type == AccountType.CreditCard && x.StatementDay is not null);
        RuleFor(x => x.DueDay!.Value)
            .InclusiveBetween((short)1, (short)28)
            .When(x => x.Type == AccountType.CreditCard && x.DueDay is not null);
    }
}
