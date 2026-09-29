using MoneyManagement.Application.Common;
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

    public TransactionService(
        ITransactionRepository transactions,
        IAccountRepository accounts,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork)
    {
        _transactions = transactions;
        _accounts = accounts;
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TransactionDto>> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
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

            var expectedCategoryType = request.Type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;
            if (category.Type != expectedCategoryType)
                return Result<TransactionDto>.Failure("Category type does not match the transaction type.", ErrorStatus.ValidationError);
        }

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
