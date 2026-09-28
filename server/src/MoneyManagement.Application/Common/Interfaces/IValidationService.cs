namespace MoneyManagement.Application.Common.Interfaces;

public interface IValidationService
{
    Task<Dictionary<string, string[]>?> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);
}
