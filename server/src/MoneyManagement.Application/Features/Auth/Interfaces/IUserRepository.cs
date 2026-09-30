using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Auth.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    void Update(User user);

    /// <summary>Newest first; null returns everyone.</summary>
    Task<IReadOnlyList<User>> ListAsync(UserStatus? status, CancellationToken cancellationToken = default);

    Task<Dictionary<UserStatus, int>> CountByStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Marks the user and everything they own (accounts, transactions, budgets, categories, settings, tokens) for deletion.</summary>
    Task RemoveWithDataAsync(User user, CancellationToken cancellationToken = default);
}
