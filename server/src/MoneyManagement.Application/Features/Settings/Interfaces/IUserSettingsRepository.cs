using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Features.Settings.Interfaces;

public interface IUserSettingsRepository
{
    /// <summary>Tracked; null until the user first saves a preference (defaults apply until then).</summary>
    Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
