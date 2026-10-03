using Microsoft.Extensions.Logging;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Accounts.Dto;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Application.Features.Accounts.Requests;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Accounts.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionService _transactionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accounts,
        ITransactionRepository transactionRepository,
        ITransactionService transactionService,
        IUnitOfWork unitOfWork,
        ILogger<AccountService> logger)
    {
        _accounts = accounts;
        _transactionRepository = transactionRepository;
        _transactionService = transactionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<AccountDto>>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _accounts.GetAllForUserAsync(userId, cancellationToken);
        var deltas = await _transactionRepository.GetBalanceDeltasAsync(userId, cancellationToken);

        var dtos = accounts
            .OrderBy(a => a.SortOrder)
            .Select(a => MapToDto(a, deltas.GetValueOrDefault(a.Id)))
            .ToList();

        return Result<IReadOnlyList<AccountDto>>.Success(dtos);
    }

    public async Task<Result<AccountDto>> CreateAsync(Guid userId, CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await _accounts.NameExistsAsync(userId, name, cancellationToken: cancellationToken))
            return Result<AccountDto>.Failure("An account with this name already exists.", ErrorStatus.Duplicate);

        var isCard = request.Type == AccountType.CreditCard;

        if (isCard && (request.CreditLimit is null || request.StatementDay is null || request.DueDay is null))
            return Result<AccountDto>.Failure("Credit limit, statement day and due day are required for a credit card.", ErrorStatus.ValidationError);

        var siblings = await _accounts.GetAllForUserAsync(userId, cancellationToken);
        var sortOrder = siblings.Count == 0 ? 0 : siblings.Max(a => a.SortOrder) + 1;

        var account = new Account
        {
            UserId = userId,
            Name = name,
            Type = request.Type,
            OpeningBalance = isCard ? -request.OpeningBalance : request.OpeningBalance,
            CreditLimit = isCard ? request.CreditLimit : null,
            StatementDay = isCard ? request.StatementDay : null,
            DueDay = isCard ? request.DueDay : null,
            Color = request.Color,
            SortOrder = sortOrder,
            CreatedBy = userId,
        };

        await _accounts.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} created {Type} account {AccountId}", userId, account.Type, account.Id);

        return Result<AccountDto>.Success(MapToDto(account, delta: 0), "Account created.");
    }

    public async Task<Result<AccountDto>> UpdateAsync(Guid userId, Guid id, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdForUserAsync(userId, id, cancellationToken);
        if (account is null)
            return Result<AccountDto>.Failure("Account not found.", ErrorStatus.NotFound);

        var name = request.Name.Trim();

        if (await _accounts.NameExistsAsync(userId, name, id, cancellationToken))
            return Result<AccountDto>.Failure("An account with this name already exists.", ErrorStatus.Duplicate);

        var isCard = account.Type == AccountType.CreditCard;

        if (isCard && (request.CreditLimit is null || request.StatementDay is null || request.DueDay is null))
            return Result<AccountDto>.Failure("Credit limit, statement day and due day are required for a credit card.", ErrorStatus.ValidationError);

        account.Name = name;
        account.OpeningBalance = isCard ? -request.OpeningBalance : request.OpeningBalance;
        account.CreditLimit = isCard ? request.CreditLimit : null;
        account.StatementDay = isCard ? request.StatementDay : null;
        account.DueDay = isCard ? request.DueDay : null;
        account.Color = request.Color;
        account.UpdatedBy = userId;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AccountDto>.Success(await BuildDtoAsync(userId, account, cancellationToken), "Account updated.");
    }

    public async Task<Result<AccountDto>> ArchiveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdForUserAsync(userId, id, cancellationToken);
        if (account is null)
            return Result<AccountDto>.Failure("Account not found.", ErrorStatus.NotFound);

        if (account.Active)
        {
            account.Active = false;
            account.UpdatedBy = userId;
            account.UpdatedAtUtc = DateTime.UtcNow;
            _accounts.Update(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} archived account {AccountId}", userId, id);
        }

        return Result<AccountDto>.Success(await BuildDtoAsync(userId, account, cancellationToken), "Account archived.");
    }

    public async Task<Result<AccountDto>> RestoreAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdForUserAsync(userId, id, cancellationToken);
        if (account is null)
            return Result<AccountDto>.Failure("Account not found.", ErrorStatus.NotFound);

        if (!account.Active)
        {
            if (await _accounts.NameExistsAsync(userId, account.Name, id, cancellationToken))
                return Result<AccountDto>.Failure("Another active account already uses this name.", ErrorStatus.Duplicate);

            account.Active = true;
            account.UpdatedBy = userId;
            account.UpdatedAtUtc = DateTime.UtcNow;
            _accounts.Update(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} restored account {AccountId}", userId, id);
        }

        return Result<AccountDto>.Success(await BuildDtoAsync(userId, account, cancellationToken), "Account restored.");
    }

    public async Task<Result<IReadOnlyList<AccountDto>>> ReorderAsync(Guid userId, ReorderAccountsRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await _accounts.GetAllForUserAsync(userId, cancellationToken);
        var byId = accounts.ToDictionary(a => a.Id);

        foreach (var id in request.AccountIds)
        {
            if (!byId.ContainsKey(id))
                return Result<IReadOnlyList<AccountDto>>.Failure("One of the accounts was not found.", ErrorStatus.NotFound);
        }

        for (var i = 0; i < request.AccountIds.Count; i++)
        {
            var account = byId[request.AccountIds[i]];
            account.SortOrder = i;
            account.UpdatedBy = userId;
            account.UpdatedAtUtc = DateTime.UtcNow;
            _accounts.Update(account);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var deltas = await _transactionRepository.GetBalanceDeltasAsync(userId, cancellationToken);
        var dtos = accounts
            .OrderBy(a => a.SortOrder)
            .Select(a => MapToDto(a, deltas.GetValueOrDefault(a.Id)))
            .ToList();

        return Result<IReadOnlyList<AccountDto>>.Success(dtos, "Accounts reordered.");
    }

    public async Task<Result<AccountDto>> PayCardAsync(Guid userId, Guid cardAccountId, PayCardRequest request, CancellationToken cancellationToken = default)
    {
        var card = await _accounts.GetByIdForUserAsync(userId, cardAccountId, cancellationToken);
        if (card is null)
            return Result<AccountDto>.Failure("Card not found.", ErrorStatus.NotFound);

        if (card.Type != AccountType.CreditCard)
            return Result<AccountDto>.Failure("Only credit cards accept payments.", ErrorStatus.ValidationError);

        var transactionResult = await _transactionService.CreateAsync(userId, new CreateTransactionRequest
        {
            Type = TransactionType.Transfer,
            TxnDate = request.Date,
            Amount = request.Amount,
            AccountId = request.SourceAccountId,
            ToAccountId = cardAccountId,
            CategoryId = null,
            Notes = request.Note,
        }, cancellationToken);

        if (!transactionResult.IsSuccess)
            return Result<AccountDto>.Failure(transactionResult.Message, transactionResult.ErrorStatus);

        _logger.LogInformation("User {UserId} paid card {AccountId} from account {SourceAccountId}", userId, cardAccountId, request.SourceAccountId);
        return Result<AccountDto>.Success(await BuildDtoAsync(userId, card, cancellationToken), "Payment recorded.");
    }

    private async Task<AccountDto> BuildDtoAsync(Guid userId, Account account, CancellationToken cancellationToken)
    {
        var deltas = await _transactionRepository.GetBalanceDeltasAsync(userId, cancellationToken);
        return MapToDto(account, deltas.GetValueOrDefault(account.Id));
    }

    private static AccountDto MapToDto(Account account, long delta)
    {
        var dto = new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Color = account.Color,
            SortOrder = account.SortOrder,
            Active = account.Active,
        };

        var balance = AccountMath.ComputeBalance(account.OpeningBalance, delta);

        if (account.Type == AccountType.CreditCard)
        {
            var outstanding = AccountMath.ComputeOutstanding(balance);
            dto.Outstanding = outstanding;
            dto.CreditLimit = account.CreditLimit;
            dto.AvailableCredit = account.CreditLimit is long limit ? AccountMath.ComputeAvailableCredit(limit, outstanding) : null;
            dto.StatementDay = account.StatementDay;
            dto.DueDay = account.DueDay;
            // ponytail: "today" is UTC, not the user's timezone; per-user timezone isn't wired
            // through this service yet. Revisit if due-date proximity needs to be exact.
            dto.NextDueDate = account.DueDay is short dueDay
                ? AccountMath.ComputeNextDueDate(dueDay, outstanding, DateOnly.FromDateTime(DateTime.UtcNow))
                : null;
        }
        else
        {
            dto.Balance = balance;
        }

        return dto;
    }
}
