namespace MoneyManagement.Application.Common.Interfaces;

public record AccessToken(string Value, int ExpiresInSeconds);

public record RawRefreshToken(string Value, string Hash, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken GenerateAccessToken(Guid userId, string email, string name);

    RawRefreshToken GenerateRefreshToken();

    string HashRefreshToken(string rawToken);
}
