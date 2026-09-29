using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Requests;

namespace MoneyManagement.Application.Features.Transactions.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionDto>> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default);
}
