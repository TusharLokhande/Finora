using FluentValidation;
using MoneyManagement.Application.Features.Transactions.Requests;

namespace MoneyManagement.Application.Features.Transactions.Validators;

public class BulkRecategorizeValidator : AbstractValidator<BulkRecategorizeRequest>
{
    public BulkRecategorizeValidator()
    {
        Include(new BulkTransactionsValidator());
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
