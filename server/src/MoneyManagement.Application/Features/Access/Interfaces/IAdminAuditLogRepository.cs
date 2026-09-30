using MoneyManagement.Application.Features.Access.Dto;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Features.Access.Interfaces;

public interface IAdminAuditLogRepository
{
    Task AddAsync(AdminAuditLog entry, CancellationToken cancellationToken = default);

    /// <summary>Newest first.</summary>
    Task<(IReadOnlyList<AuditLogDto> Items, int TotalCount)> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
}
