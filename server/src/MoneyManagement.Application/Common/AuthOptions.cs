namespace MoneyManagement.Application.Common;

public class AuthOptions
{
    public List<string> AllowedEmails { get; set; } = [];
    public string FrontendBaseUrl { get; set; } = string.Empty;
    public string PostLoginPath { get; set; } = "/auth/callback";
    public string LoginErrorPath { get; set; } = "/login";
}
