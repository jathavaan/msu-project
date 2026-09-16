namespace FinanceOne.Api.Common.BlobStorage;

/// <summary>
/// Everything a slice needs to keep a blob in one of the containers in <see cref="BlobContainers"/>.
/// The only way anything in this API touches Blob Storage — no slice talks to
/// <see cref="Azure.Storage.Blobs.BlobServiceClient"/> directly, and nothing outside the API is ever
/// handed a SAS token: uploads and downloads are proxied through the API's own endpoints (see
/// Features/DiscountCodes/UploadDiscountCodeImage and GetDiscountCodeImage).
/// </summary>
public interface IBlobStorageService
{
    Task UploadAsync(string container, string blobName, Stream content, string contentType, CancellationToken cancellationToken);

    /// <summary>Null when no blob exists at that name.</summary>
    Task<DownloadedBlob?> DownloadAsync(string container, string blobName, CancellationToken cancellationToken);

    /// <summary>No-op if the blob doesn't exist.</summary>
    Task DeleteAsync(string container, string blobName, CancellationToken cancellationToken);
}
