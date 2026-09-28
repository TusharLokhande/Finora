using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Categories.Dto;
using MoneyManagement.Application.Features.Categories.Requests;

namespace MoneyManagement.Application.Features.Categories.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> ArchiveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> RestoreAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
