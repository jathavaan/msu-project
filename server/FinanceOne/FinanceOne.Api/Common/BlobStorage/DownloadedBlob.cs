namespace FinanceOne.Api.Common.BlobStorage;

/// <summary>Content streamed back from <see cref="IBlobStorageService.DownloadAsync"/>.</summary>
public sealed record DownloadedBlob(Stream Content, string ContentType);
