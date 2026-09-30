using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Requests;

namespace MoneyManagement.Application.Features.Transactions.Interfaces;

public interface ITransactionService
{
    Task<Result<PageResult<TransactionDto>>> SearchAsync(Guid userId, PageRequest<TransactionFilterRequest> request, CancellationToken cancellationToken = default);

    /// <summary>Newest <paramref name="limit"/> transactions, via the same query as <see cref="SearchAsync"/>.</summary>
    Task<Result<IReadOnlyList<TransactionDto>>> GetRecentAsync(Guid userId, int limit, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DescriptionSuggestionDto>>> GetDescriptionSuggestionsAsync(Guid userId, string prefix, CancellationToken cancellationToken = default);

    /// <summary>Writes every transaction matching <paramref name="filter"/> (all of them when null) as .xlsx. Returns the row count.</summary>
    Task<Result<int>> ExportAsync(Guid userId, TransactionFilterRequest? filter, Stream output, CancellationToken cancellationToken = default);

    Task<Result<TransactionDto>> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default);

    Task<Result<TransactionDto>> UpdateAsync(Guid userId, Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Soft delete (Active = false); undone by <see cref="RestoreAsync"/>.</summary>
    Task<Result<TransactionDto>> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<Result<TransactionDto>> RestoreAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes all of the given transactions, or none if any id is unknown. Returns the count.</summary>
    Task<Result<int>> BulkDeleteAsync(Guid userId, BulkTransactionsRequest request, CancellationToken cancellationToken = default);

    Task<Result<int>> BulkRestoreAsync(Guid userId, BulkTransactionsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Moves all of the given transactions to one category, or none if any breaks the category rules.</summary>
    Task<Result<int>> BulkRecategorizeAsync(Guid userId, BulkRecategorizeRequest request, CancellationToken cancellationToken = default);
}
