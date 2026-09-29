using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(a => a.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(Guid userId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.ToLower();

        return await _dbSet.AnyAsync(
            a => a.UserId == userId
                && a.Active
                && a.Id != excludeId
                && a.Name.ToLower() == normalized,
            cancellationToken);
    }
}
