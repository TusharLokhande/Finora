using FluentValidation;
using MoneyManagement.Application.Features.Accounts.Requests;

namespace MoneyManagement.Application.Features.Accounts.Validators;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a hex value like #F97316.")
            .When(x => !string.IsNullOrEmpty(x.Color));

        // Whether these are required/forbidden depends on the account's stored Type,
        // which this request-shape validator can't see — AccountService enforces that part.
        RuleFor(x => x.CreditLimit!.Value).GreaterThan(0).When(x => x.CreditLimit is not null);
        RuleFor(x => x.StatementDay!.Value).InclusiveBetween((short)1, (short)28).When(x => x.StatementDay is not null);
        RuleFor(x => x.DueDay!.Value).InclusiveBetween((short)1, (short)28).When(x => x.DueDay is not null);
    }
}
