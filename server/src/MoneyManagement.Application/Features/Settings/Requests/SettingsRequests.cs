using System.Text.Json;
using System.Text.Json.Serialization;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Settings.Requests;

public record UpdateProfileRequest(string Name);

public class UpdatePreferencesRequest
{
    public string Timezone { get; set; } = string.Empty;
    public Theme Theme { get; set; }
    public Density Density { get; set; }

    /// <summary>Any property we don't model (notably currencyCode) lands here so the service can reject it instead of silently ignoring it.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}

public record DeleteAccountRequest(string Confirmation);
