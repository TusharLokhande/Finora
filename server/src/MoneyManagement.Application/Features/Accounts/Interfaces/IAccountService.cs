using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Accounts.Dto;
using MoneyManagement.Application.Features.Accounts.Requests;

namespace MoneyManagement.Application.Features.Accounts.Interfaces;

public interface IAccountService
{
    Task<Result<IReadOnlyList<AccountDto>>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<AccountDto>> CreateAsync(Guid userId, CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<Result<AccountDto>> UpdateAsync(Guid userId, Guid id, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    Task<Result<AccountDto>> ArchiveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<Result<AccountDto>> RestoreAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AccountDto>>> ReorderAsync(Guid userId, ReorderAccountsRequest request, CancellationToken cancellationToken = default);

    Task<Result<AccountDto>> PayCardAsync(Guid userId, Guid cardAccountId, PayCardRequest request, CancellationToken cancellationToken = default);
}
