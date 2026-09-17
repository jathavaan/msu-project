using Azure.Storage.Blobs;
using FinanceOne.Api.Common.BlobStorage;
using Testcontainers.Azurite;

namespace FinanceOne.IntegrationTests.Common;

/// <summary>
/// Starts one throwaway Azurite container for the whole test assembly — the same emulator
/// docker-compose runs for local dev — and pre-creates the containers infra/modules/storage-app.bicep
/// creates in the real account, so BlobStorageService behaves identically to production.
/// </summary>
public sealed class AzuriteFixture : IAsyncLifetime
{
    // Testcontainers.Azurite's own default image (3.28.0) is old enough that the current
    // Azure.Storage.Blobs SDK's default service version is rejected outright
    // ("The API version 2025-05-05 is not supported by Azurite"). Pin the same newer tag
    // docker-compose.yml runs for local dev instead.
    private readonly AzuriteContainer _container =
        new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.34.0").Build();

    public IBlobStorageService BlobStorageService { get; private set; } = null!;

    // Exposed alongside BlobStorageService (rather than only through it) for tests that need to
    // enumerate a container's blobs — e.g. finding a generated-name blob a slice just staged.
    // IBlobStorageService deliberately has no listing operation (nothing outside the API is ever
    // handed a SAS token or the container's contents), so tests reach for the raw client instead.
    public BlobServiceClient BlobServiceClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        BlobServiceClient = new BlobServiceClient(_container.GetConnectionString());
        foreach (var container in new[]
                 { BlobContainers.Coupons, BlobContainers.Exports, BlobContainers.StagedCsv })
        {
            await BlobServiceClient.GetBlobContainerClient(container).CreateIfNotExistsAsync();
        }

        BlobStorageService = new BlobStorageService(BlobServiceClient);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
