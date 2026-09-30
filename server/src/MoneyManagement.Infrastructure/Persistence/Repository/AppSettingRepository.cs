using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Access.Interfaces;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class AppSettingRepository : IAppSettingRepository
{
    private const string SignupsOpenKey = "signups_open";

    private readonly AppDbContext _context;

    public AppSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> GetSignupsOpenAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == SignupsOpenKey, cancellationToken);
        return setting is null || setting.Value != bool.FalseString;
    }

    public async Task SetSignupsOpenAsync(bool open, CancellationToken cancellationToken = default)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == SignupsOpenKey, cancellationToken);
        if (setting is null)
            await _context.AppSettings.AddAsync(new AppSetting { Key = SignupsOpenKey, Value = open.ToString() }, cancellationToken);
        else
            setting.Value = open.ToString();
    }
}
