using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Settings.Interfaces;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly AppDbContext _context;

    public UserSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        await _context.UserSettings.AddAsync(settings, cancellationToken);
    }
}
