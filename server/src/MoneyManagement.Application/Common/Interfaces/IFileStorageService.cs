namespace MoneyManagement.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task SaveAsync(string key, Stream content, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
