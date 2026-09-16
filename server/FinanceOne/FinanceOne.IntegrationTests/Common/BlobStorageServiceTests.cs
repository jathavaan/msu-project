using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.IntegrationTests.Common;

// Not a slice — this is the cross-cutting plumbing every slice that touches Blob Storage builds
// on (see UploadDiscountCodeImage/GetDiscountCodeImage), so it gets its own test class here rather
// than living under Features/.
[Collection(DatabaseCollection.Name)]
public class BlobStorageServiceTests(AzuriteFixture fixture)
{
    private IBlobStorageService Service => fixture.BlobStorageService;

    [Fact]
    public async Task Round_Trips_Content_And_Content_Type()
    {
        var blobName = Guid.NewGuid().ToString();
        var bytes = new byte[] { 1, 2, 3, 4 };

        await Service.UploadAsync(BlobContainers.Coupons, blobName, new MemoryStream(bytes), "image/png", CancellationToken.None);
        var downloaded = await Service.DownloadAsync(BlobContainers.Coupons, blobName, CancellationToken.None);

        Assert.NotNull(downloaded);
        Assert.Equal("image/png", downloaded.ContentType);
        await using var content = new MemoryStream();
        await downloaded.Content.CopyToAsync(content);
        Assert.Equal(bytes, content.ToArray());
    }

    [Fact]
    public async Task DownloadAsync_Returns_Null_When_The_Blob_Does_Not_Exist()
    {
        var downloaded = await Service.DownloadAsync(BlobContainers.Coupons, Guid.NewGuid().ToString(), CancellationToken.None);

        Assert.Null(downloaded);
    }

    [Fact]
    public async Task Uploading_To_The_Same_Name_Overwrites_The_Previous_Blob()
    {
        var blobName = Guid.NewGuid().ToString();
        await Service.UploadAsync(BlobContainers.Coupons, blobName, new MemoryStream([1]), "image/png", CancellationToken.None);

        await Service.UploadAsync(BlobContainers.Coupons, blobName, new MemoryStream([2, 2]), "image/jpeg", CancellationToken.None);

        var downloaded = await Service.DownloadAsync(BlobContainers.Coupons, blobName, CancellationToken.None);
        Assert.Equal("image/jpeg", downloaded!.ContentType);
        await using var content = new MemoryStream();
        await downloaded.Content.CopyToAsync(content);
        Assert.Equal([2, 2], content.ToArray());
    }

    [Fact]
    public async Task DeleteAsync_Removes_The_Blob()
    {
        var blobName = Guid.NewGuid().ToString();
        await Service.UploadAsync(BlobContainers.Coupons, blobName, new MemoryStream([1]), "image/png", CancellationToken.None);

        await Service.DeleteAsync(BlobContainers.Coupons, blobName, CancellationToken.None);

        Assert.Null(await Service.DownloadAsync(BlobContainers.Coupons, blobName, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Is_A_No_Op_When_The_Blob_Does_Not_Exist()
    {
        await Service.DeleteAsync(BlobContainers.Coupons, Guid.NewGuid().ToString(), CancellationToken.None);
    }
}
