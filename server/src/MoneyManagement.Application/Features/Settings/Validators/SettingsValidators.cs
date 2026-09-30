using FluentValidation;
using MoneyManagement.Application.Features.Settings.Requests;

namespace MoneyManagement.Application.Features.Settings.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
