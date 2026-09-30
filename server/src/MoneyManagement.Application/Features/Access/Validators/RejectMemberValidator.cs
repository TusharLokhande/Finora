using FluentValidation;
using MoneyManagement.Application.Features.Access.Requests;

namespace MoneyManagement.Application.Features.Access.Validators;

public class RejectMemberValidator : AbstractValidator<RejectMemberRequest>
{
    public RejectMemberValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
