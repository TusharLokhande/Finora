using FluentValidation;
using MoneyManagement.Application.Features.Transactions.Requests;

namespace MoneyManagement.Application.Features.Transactions.Validators;

public class BulkTransactionsValidator : AbstractValidator<BulkTransactionsRequest>
{
    public const int MaxIds = 500;

    public BulkTransactionsValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().WithMessage("Select at least one transaction.");
        RuleFor(x => x.Ids.Count).LessThanOrEqualTo(MaxIds).WithMessage($"At most {MaxIds} transactions at a time.");
    }
}
