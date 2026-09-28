using FluentValidation;
using MoneyManagement.Application.Features.Categories.Requests;

namespace MoneyManagement.Application.Features.Categories.Validators;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a hex value like #F97316.")
            .When(x => !string.IsNullOrEmpty(x.Color));
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}
