using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Settings.Dto;
using MoneyManagement.Application.Features.Settings.Requests;

namespace MoneyManagement.Application.Features.Settings.Interfaces;

public interface ISettingsService
{
    Task<Result<SettingsDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<SettingsDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task<Result<SettingsDto>> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserExportDto>> ExportAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<object?>> DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default);
}
