using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.SavingGoals.UploadSavingGoalImage;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.UploadSavingGoalImage;

public class UploadSavingGoalImageTests(MySqlFixture mySqlFixture, AzuriteFixture azuriteFixture)
    : IntegrationTest(mySqlFixture)
{
    private UploadSavingGoalImageHandler Handler =>
        new(new UploadSavingGoalImageRepository(Context), azuriteFixture.BlobStorageService);

    [Fact]
    public async Task Returns_404_When_The_Saving_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(
            new UploadSavingGoalImageCommand(Guid.NewGuid(), new MemoryStream([1]), "image/png", 1),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Uploads_The_Image_And_Persists_The_Proxied_ImageUrl()
    {
        var savingGoal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var bytes = new byte[] { 1, 2, 3 };

        var response = await Handler.Handle(
            new UploadSavingGoalImageCommand(savingGoal.Id, new MemoryStream(bytes), "image/png", bytes.Length),
            CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        var saved = await context.SavingGoals.SingleAsync(s => s.Id == savingGoal.Id);
        Assert.Equal($"/api/saving-goals/{savingGoal.Id}/image", saved.ImageUrl);

        var downloaded = await azuriteFixture.BlobStorageService.DownloadAsync(
            BlobContainers.SavingGoalImages, savingGoal.Id.ToString(), CancellationToken.None);
        Assert.NotNull(downloaded);
        Assert.Equal("image/png", downloaded.ContentType);
    }

    [Fact]
    public async Task Uploading_Again_Replaces_The_Previous_Image()
    {
        var savingGoal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        await Handler.Handle(
            new UploadSavingGoalImageCommand(savingGoal.Id, new MemoryStream([1]), "image/png", 1),
            CancellationToken.None);

        await Handler.Handle(
            new UploadSavingGoalImageCommand(savingGoal.Id, new MemoryStream([2, 2]), "image/jpeg", 2),
            CancellationToken.None);

        var downloaded = await azuriteFixture.BlobStorageService.DownloadAsync(
            BlobContainers.SavingGoalImages, savingGoal.Id.ToString(), CancellationToken.None);
        Assert.Equal("image/jpeg", downloaded!.ContentType);
    }
}
