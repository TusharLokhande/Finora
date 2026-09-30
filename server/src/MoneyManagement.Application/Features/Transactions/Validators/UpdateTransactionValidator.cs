using FluentValidation;
using MoneyManagement.Application.Features.Transactions.Requests;

namespace MoneyManagement.Application.Features.Transactions.Validators;

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        Include(new CreateTransactionValidator());
    }
}
