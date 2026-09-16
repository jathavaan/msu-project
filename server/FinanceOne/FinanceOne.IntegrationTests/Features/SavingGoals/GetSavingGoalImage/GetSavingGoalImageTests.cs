using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;
using FinanceOne.IntegrationTests.Common;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.GetSavingGoalImage;

public class GetSavingGoalImageTests(MySqlFixture mySqlFixture, AzuriteFixture azuriteFixture)
    : IntegrationTest(mySqlFixture)
{
    private GetSavingGoalImageHandler Handler =>
        new(new GetSavingGoalImageRepository(Context), azuriteFixture.BlobStorageService);

    [Fact]
    public async Task Returns_404_When_The_Saving_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(new GetSavingGoalImageQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_404_When_No_Image_Has_Been_Uploaded()
    {
        var savingGoal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        var response = await Handler.Handle(new GetSavingGoalImageQuery(savingGoal.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Streams_Back_A_Previously_Uploaded_Image()
    {
        var savingGoal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var bytes = new byte[] { 9, 8, 7 };
        await azuriteFixture.BlobStorageService.UploadAsync(
            BlobContainers.SavingGoalImages, savingGoal.Id.ToString(), new MemoryStream(bytes), "image/webp", CancellationToken.None);

        var response = await Handler.Handle(new GetSavingGoalImageQuery(savingGoal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("image/webp", response.Result!.ContentType);
        await using var content = new MemoryStream();
        await response.Result.Content.CopyToAsync(content);
        Assert.Equal(bytes, content.ToArray());
    }
}
