using MoneyManagement.Application.Interfaces.Repository;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Features.Accounts.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Account?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(Guid userId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
