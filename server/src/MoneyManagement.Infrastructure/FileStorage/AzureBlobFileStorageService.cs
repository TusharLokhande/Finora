using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using MoneyManagement.Application.Common.Interfaces;

namespace MoneyManagement.Infrastructure.FileStorage;

public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobFileStorageService(BlobServiceClient blobServiceClient, IOptions<FileStorageOptions> options)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
    }

    public async Task SaveAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        content.Position = 0;
        await _containerClient.GetBlobClient(key).UploadAsync(content, overwrite: true, cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var download = await _containerClient.GetBlobClient(key).DownloadStreamingAsync(cancellationToken: cancellationToken);
        return download.Value.Content;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await _containerClient.GetBlobClient(key).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
