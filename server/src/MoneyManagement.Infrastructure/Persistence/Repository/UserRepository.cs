using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Auth.Interfaces;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task<IReadOnlyList<User>> ListAsync(UserStatus? status, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => status == null || u.Status == status)
            .OrderByDescending(u => u.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<UserStatus, int>> CountByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .GroupBy(u => u.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    // The composite FKs between transactions/budgets and accounts/categories are RESTRICT, so a bare
    // user delete can trip on cascade order in Postgres. Loading the dependents lets EF order the deletes.
    public async Task RemoveWithDataAsync(User user, CancellationToken cancellationToken = default)
    {
        var id = user.Id;
        _context.Transactions.RemoveRange(await _context.Transactions.Where(t => t.UserId == id).ToListAsync(cancellationToken));
        _context.Budgets.RemoveRange(await _context.Budgets.Where(b => b.UserId == id).ToListAsync(cancellationToken));
        _context.Categories.RemoveRange(await _context.Categories.Where(c => c.UserId == id).ToListAsync(cancellationToken));
        _context.Accounts.RemoveRange(await _context.Accounts.Where(a => a.UserId == id).ToListAsync(cancellationToken));
        _context.RefreshTokens.RemoveRange(await _context.RefreshTokens.Where(r => r.UserId == id).ToListAsync(cancellationToken));
        _context.UserSettings.RemoveRange(await _context.UserSettings.Where(s => s.UserId == id).ToListAsync(cancellationToken));
        _context.Users.Remove(user);
    }
}
