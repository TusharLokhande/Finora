using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Settings.Interfaces;
using MoneyManagement.Application.Features.Settings.Requests;

namespace MoneyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private static readonly JsonSerializerOptions ExportJson = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ISettingsService _settingsService;
    private readonly ICurrentUserService _currentUser;

    public SettingsController(ISettingsService settingsService, ICurrentUserService currentUser)
    {
        _settingsService = settingsService;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId!.Value;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => (await _settingsService.GetAsync(UserId, cancellationToken)).ToActionResult();

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        => (await _settingsService.UpdateProfileAsync(UserId, request, cancellationToken)).ToActionResult();

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken)
        => (await _settingsService.UpdatePreferencesAsync(UserId, request, cancellationToken)).ToActionResult();

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var result = await _settingsService.ExportAsync(UserId, cancellationToken);
        if (!result.IsSuccess)
            return result.ToActionResult();

        return File(JsonSerializer.SerializeToUtf8Bytes(result.Data, ExportJson), "application/json", $"finora-export-{DateTime.UtcNow:yyyy-MM-dd}.json");
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _settingsService.DeleteAccountAsync(UserId, request, cancellationToken);
        if (result.IsSuccess)
            Response.Cookies.Delete(AuthController.RefreshCookieName);

        return result.ToActionResult();
    }
}
