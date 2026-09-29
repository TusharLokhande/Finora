using FluentValidation;
using MoneyManagement.Application.Features.Accounts.Requests;

namespace MoneyManagement.Application.Features.Accounts.Validators;

public class PayCardValidator : AbstractValidator<PayCardRequest>
{
    public PayCardValidator()
    {
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Date).LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Date can't be in the future.");
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
