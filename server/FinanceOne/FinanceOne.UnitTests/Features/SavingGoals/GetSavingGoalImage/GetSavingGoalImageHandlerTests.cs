using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;

namespace FinanceOne.UnitTests.Features.SavingGoals.GetSavingGoalImage;

public class GetSavingGoalImageHandlerTests
{
    private readonly IGetSavingGoalImageRepository _repository = Substitute.For<IGetSavingGoalImageRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    private GetSavingGoalImageHandler Handler => new(_repository, _blobStorage);

    private static SavingGoal NewSavingGoal(Guid id) => new()
    {
        Id = id,
        Name = "New Car",
        TargetAmount = 250_000m,
        TargetDate = new DateOnly(2027, 6, 15),
    };

    [Fact]
    public async Task Returns_404_When_The_Saving_Goal_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SavingGoal?)null);

        var response = await Handler.Handle(new GetSavingGoalImageQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _blobStorage.DidNotReceive().DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_When_No_Image_Has_Been_Uploaded()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(NewSavingGoal(id));
        _blobStorage.DownloadAsync(BlobContainers.SavingGoalImages, id.ToString(), Arg.Any<CancellationToken>())
            .Returns((DownloadedBlob?)null);

        var response = await Handler.Handle(new GetSavingGoalImageQuery(id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Downloaded_Image()
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(NewSavingGoal(id));
        var blob = new DownloadedBlob(new MemoryStream([1, 2, 3]), "image/png");
        _blobStorage.DownloadAsync(BlobContainers.SavingGoalImages, id.ToString(), Arg.Any<CancellationToken>()).Returns(blob);

        var response = await Handler.Handle(new GetSavingGoalImageQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Same(blob, response.Result);
    }
}
