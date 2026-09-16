using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FinanceOne.Api.Common.BlobStorage;

public sealed class BlobStorageService(BlobServiceClient blobServiceClient) : IBlobStorageService
{
    public async Task UploadAsync(string container, string blobName, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blobName);
        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            cancellationToken);
    }

    public async Task<DownloadedBlob?> DownloadAsync(string container, string blobName, CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var download = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return new DownloadedBlob(download.Value.Content, download.Value.Details.ContentType);
    }

    public async Task DeleteAsync(string container, string blobName, CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
