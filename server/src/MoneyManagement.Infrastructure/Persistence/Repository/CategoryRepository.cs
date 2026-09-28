using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Category>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetSiblingsAsync(Guid userId, Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.UserId == userId && c.ParentId == parentId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.ParentId == parentId && c.Active).ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(Guid userId, Guid? parentId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.ToLower();

        return await _dbSet.AnyAsync(
            c => c.UserId == userId
                && c.ParentId == parentId
                && c.Active
                && c.Id != excludeId
                && c.Name.ToLower() == normalized,
            cancellationToken);
    }
}
