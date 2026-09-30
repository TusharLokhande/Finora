namespace MoneyManagement.Application.Features.Access.Interfaces;

public interface IAppSettingRepository
{
    /// <summary>Whether the Google callback may create new Pending users. Open when never set.</summary>
    Task<bool> GetSignupsOpenAsync(CancellationToken cancellationToken = default);

    Task SetSignupsOpenAsync(bool open, CancellationToken cancellationToken = default);
}
