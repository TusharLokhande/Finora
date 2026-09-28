using MoneyManagement.Application.Interfaces.Repository;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Features.Categories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetSiblingsAsync(Guid userId, Guid? parentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetActiveChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(Guid userId, Guid? parentId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
