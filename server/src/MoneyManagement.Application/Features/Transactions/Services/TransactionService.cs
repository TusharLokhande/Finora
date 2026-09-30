using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Common.Export;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Transactions.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactions;
    private readonly IAccountRepository _accounts;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelExportWriter _excel;

    public TransactionService(
        ITransactionRepository transactions,
        IAccountRepository accounts,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        IExcelExportWriter excel)
    {
        _transactions = transactions;
        _accounts = accounts;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _excel = excel;
    }

    private const int MaxPageSize = 100;
    private const int MaxRecent = 50;
    private const int SuggestionLimit = 8;

    public async Task<Result<PageResult<TransactionDto>>> SearchAsync(Guid userId, PageRequest<TransactionFilterRequest> request, CancellationToken cancellationToken = default)
    {
        request.Page = Math.Max(request.Page, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var page = await _transactions.SearchAsync(userId, request, cancellationToken);
        return Result<PageResult<TransactionDto>>.Success(page);
    }

    public async Task<Result<IReadOnlyList<TransactionDto>>> GetRecentAsync(Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        var page = await _transactions.SearchAsync(userId, new PageRequest<TransactionFilterRequest>
        {
            Page = 1,
            PageSize = Math.Clamp(limit, 1, MaxRecent),
        }, cancellationToken);

        return Result<IReadOnlyList<TransactionDto>>.Success(page.Items);
    }

    public async Task<Result<IReadOnlyList<DescriptionSuggestionDto>>> GetDescriptionSuggestionsAsync(Guid userId, string prefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return Result<IReadOnlyList<DescriptionSuggestionDto>>.Success([]);

        var suggestions = await _transactions.GetDescriptionSuggestionsAsync(userId, prefix, SuggestionLimit, cancellationToken);
        return Result<IReadOnlyList<DescriptionSuggestionDto>>.Success(suggestions);
    }

    public async Task<Result<int>> ExportAsync(Guid userId, TransactionFilterRequest? filter, Stream output, CancellationToken cancellationToken = default)
    {
        // Same query as the list, unpaged.
        var all = await _transactions.SearchAsync(userId, new PageRequest<TransactionFilterRequest>
        {
            Page = 1,
            PageSize = int.MaxValue,
            CustomFilter = filter,
        }, cancellationToken);

        await _excel.WriteAsync([ToSheet(all.Items)], output, cancellationToken);
        return Result<int>.Success(all.Items.Count);
    }

    public Task<Result<int>> BulkDeleteAsync(Guid userId, BulkTransactionsRequest request, CancellationToken cancellationToken = default)
        => BulkSetActiveAsync(userId, request.Ids, false, "deleted", cancellationToken);

    public Task<Result<int>> BulkRestoreAsync(Guid userId, BulkTransactionsRequest request, CancellationToken cancellationToken = default)
        => BulkSetActiveAsync(userId, request.Ids, true, "restored", cancellationToken);

    private async Task<Result<int>> BulkSetActiveAsync(Guid userId, List<Guid> ids, bool active, string verb, CancellationToken cancellationToken)
    {
        var distinct = ids.Distinct().ToList();
        var transactions = await _transactions.GetByIdsForUserAsync(userId, distinct, cancellationToken);
        if (transactions.Count != distinct.Count)
            return Result<int>.Failure("One or more transactions were not found.", ErrorStatus.NotFound);

        foreach (var transaction in transactions.Where(t => t.Active != active))
        {
            transaction.Active = active;
            transaction.UpdatedBy = userId;
            transaction.UpdatedAtUtc = DateTime.UtcNow;
            _transactions.Update(transaction);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(transactions.Count, $"{transactions.Count} transaction(s) {verb}.");
    }

    public async Task<Result<int>> BulkRecategorizeAsync(Guid userId, BulkRecategorizeRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdForUserAsync(userId, request.CategoryId, cancellationToken);
        if (category is null)
            return Result<int>.Failure("Category not found.", ErrorStatus.NotFound);

        var distinct = request.Ids.Distinct().ToList();
        var transactions = await _transactions.GetByIdsForUserAsync(userId, distinct, cancellationToken);
        if (transactions.Count != distinct.Count || transactions.Any(t => !t.Active))
            return Result<int>.Failure("One or more transactions were not found.", ErrorStatus.NotFound);

        if (transactions.Any(t => t.Type == TransactionType.Transfer))
            return Result<int>.Failure("Transfers cannot have a category.", ErrorStatus.ValidationError);

        if (transactions.Any(t => ExpectedCategoryType(t.Type) != category.Type))
            return Result<int>.Failure("Category type does not match the transaction type.", ErrorStatus.ValidationError);

        foreach (var transaction in transactions)
        {
            transaction.CategoryId = category.Id;
            transaction.UpdatedBy = userId;
            transaction.UpdatedAtUtc = DateTime.UtcNow;
            _transactions.Update(transaction);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(transactions.Count, $"{transactions.Count} transaction(s) re-categorized.");
    }

    private static readonly ExcelColumn[] ExportColumns =
    [
        new("Date", ExcelFormat.Date),
        new("Type"),
        new("Description"),
        new("Category"),
        new("Sub-category"),
        new("Account"),
        new("Amount", ExcelFormat.Money),
        new("Notes"),
    ];

    /// <summary>A sub-category's parent goes in "Category"; a top-level category leaves "Sub-category" empty.</summary>
    private static ExcelSheet ToSheet(IReadOnlyList<TransactionDto> rows) => new("Transactions", ExportColumns,
        rows.Select(t => new object?[]
        {
            t.TxnDate,
            t.Type.ToString(),
            t.Description,
            t.ParentCategoryName ?? t.CategoryName,
            t.ParentCategoryName is null ? null : t.CategoryName,
            t.Type == TransactionType.Transfer ? $"{t.AccountName} → {t.ToAccountName}" : t.AccountName,
            t.Amount,
            t.Notes,
        }).ToList());

    private static CategoryType ExpectedCategoryType(TransactionType type)
        => type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;

    public async Task<Result<TransactionDto>> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (await ValidateReferencesAsync(userId, request, cancellationToken) is { } failure)
            return failure;

        var transaction = new Transaction
        {
            UserId = userId,
            Type = request.Type,
            TxnDate = request.TxnDate,
            Amount = request.Amount,
            AccountId = request.AccountId,
            ToAccountId = request.ToAccountId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            Notes = request.Notes,
            CreatedBy = userId,
        };

        await _transactions.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TransactionDto>.Success(MapToDto(transaction), "Transaction created.");
    }

    public async Task<Result<TransactionDto>> UpdateAsync(Guid userId, Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactions.GetByIdForUserAsync(userId, id, cancellationToken);
        if (transaction is null || !transaction.Active)
            return Result<TransactionDto>.Failure("Transaction not found.", ErrorStatus.NotFound);

        if (await ValidateReferencesAsync(userId, request, cancellationToken) is { } failure)
            return failure;

        transaction.Type = request.Type;
        transaction.TxnDate = request.TxnDate;
        transaction.Amount = request.Amount;
        transaction.AccountId = request.AccountId;
        transaction.ToAccountId = request.ToAccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.Description = request.Description;
        transaction.Notes = request.Notes;
        transaction.UpdatedBy = userId;
        transaction.UpdatedAtUtc = DateTime.UtcNow;

        _transactions.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TransactionDto>.Success(MapToDto(transaction), "Transaction updated.");
    }

    public Task<Result<TransactionDto>> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(userId, id, false, "Transaction deleted.", cancellationToken);

    public Task<Result<TransactionDto>> RestoreAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(userId, id, true, "Transaction restored.", cancellationToken);

    private async Task<Result<TransactionDto>> SetActiveAsync(Guid userId, Guid id, bool active, string message, CancellationToken cancellationToken)
    {
        var transaction = await _transactions.GetByIdForUserAsync(userId, id, cancellationToken);
        if (transaction is null)
            return Result<TransactionDto>.Failure("Transaction not found.", ErrorStatus.NotFound);

        if (transaction.Active != active)
        {
            transaction.Active = active;
            transaction.UpdatedBy = userId;
            transaction.UpdatedAtUtc = DateTime.UtcNow;
            _transactions.Update(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<TransactionDto>.Success(MapToDto(transaction), message);
    }

    /// <summary>Checks the rules FluentValidation can't: ownership of accounts/categories and category type. Null when valid.</summary>
    private async Task<Result<TransactionDto>?> ValidateReferencesAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return Result<TransactionDto>.Failure("Amount must be greater than zero.", ErrorStatus.ValidationError);

        var account = await _accounts.GetByIdForUserAsync(userId, request.AccountId, cancellationToken);
        if (account is null)
            return Result<TransactionDto>.Failure("Account not found.", ErrorStatus.NotFound);

        if (request.Type == TransactionType.Transfer)
        {
            if (request.ToAccountId is null)
                return Result<TransactionDto>.Failure("A transfer needs a destination account.", ErrorStatus.ValidationError);

            if (request.ToAccountId == request.AccountId)
                return Result<TransactionDto>.Failure("Source and destination accounts must be different.", ErrorStatus.ValidationError);

            if (request.CategoryId is not null)
                return Result<TransactionDto>.Failure("Transfers cannot have a category.", ErrorStatus.ValidationError);

            var toAccount = await _accounts.GetByIdForUserAsync(userId, request.ToAccountId.Value, cancellationToken);
            if (toAccount is null)
                return Result<TransactionDto>.Failure("Destination account not found.", ErrorStatus.NotFound);
        }
        else
        {
            if (request.ToAccountId is not null)
                return Result<TransactionDto>.Failure("Only transfers can have a destination account.", ErrorStatus.ValidationError);

            if (request.CategoryId is null)
                return Result<TransactionDto>.Failure("A category is required.", ErrorStatus.ValidationError);

            var category = await _categories.GetByIdForUserAsync(userId, request.CategoryId.Value, cancellationToken);
            if (category is null)
                return Result<TransactionDto>.Failure("Category not found.", ErrorStatus.NotFound);

            if (category.Type != ExpectedCategoryType(request.Type))
                return Result<TransactionDto>.Failure("Category type does not match the transaction type.", ErrorStatus.ValidationError);
        }

        return null;
    }

    private static TransactionDto MapToDto(Transaction transaction) => new()
    {
        Id = transaction.Id,
        Type = transaction.Type,
        TxnDate = transaction.TxnDate,
        Amount = transaction.Amount,
        AccountId = transaction.AccountId,
        ToAccountId = transaction.ToAccountId,
        CategoryId = transaction.CategoryId,
        Description = transaction.Description,
        Notes = transaction.Notes,
    };
}
