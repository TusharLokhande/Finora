namespace MoneyManagement.Application.Common;

public class AuthOptions
{
    public string AdminEmail { get; set; } = string.Empty;
    public string FrontendBaseUrl { get; set; } = string.Empty;
    public string PostLoginPath { get; set; } = "/auth/callback";
    public string LoginErrorPath { get; set; } = "/login";
}
