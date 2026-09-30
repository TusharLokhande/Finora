using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Access.Dto;
using MoneyManagement.Application.Features.Access.Interfaces;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class AdminAuditLogRepository : IAdminAuditLogRepository
{
    private readonly AppDbContext _context;

    public AdminAuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AdminAuditLog entry, CancellationToken cancellationToken = default)
    {
        await _context.AdminAuditLogs.AddAsync(entry, cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLogDto> Items, int TotalCount)> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var total = await _context.AdminAuditLogs.CountAsync(cancellationToken);

        var items = await _context.AdminAuditLogs
            .OrderByDescending(l => l.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .Join(_context.Users, l => l.AdminUserId, u => u.Id,
                (l, admin) => new AuditLogDto(l.Id, l.Action, admin.Email, l.TargetEmail, l.Reason, l.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
