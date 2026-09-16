using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.SavingGoals.UploadSavingGoalImage;

namespace FinanceOne.UnitTests.Features.SavingGoals.UploadSavingGoalImage;

public class UploadSavingGoalImageHandlerTests
{
    private readonly IUploadSavingGoalImageRepository _repository = Substitute.For<IUploadSavingGoalImageRepository>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();

    private UploadSavingGoalImageHandler Handler => new(_repository, _blobStorage);

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

        var response = await Handler.Handle(
            new UploadSavingGoalImageCommand(Guid.NewGuid(), new MemoryStream(), "image/png", 10),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        await _blobStorage.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("application/pdf")]
    public async Task Returns_400_When_The_Content_Type_Is_Not_An_Image(string? contentType)
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(NewSavingGoal(id));

        var response = await Handler.Handle(
            new UploadSavingGoalImageCommand(id, new MemoryStream(), contentType, 10),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        await _blobStorage.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5 * 1024 * 1024 + 1)]
    public async Task Returns_400_When_The_File_Size_Is_Out_Of_Range(long length)
    {
        var id = Guid.NewGuid();
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(NewSavingGoal(id));

        var response = await Handler.Handle(
            new UploadSavingGoalImageCommand(id, new MemoryStream(), "image/png", length),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        await _blobStorage.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().Update(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Uploads_The_Image_And_Points_ImageUrl_At_The_Proxied_Endpoint()
    {
        var id = Guid.NewGuid();
        var savingGoal = NewSavingGoal(id);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(savingGoal);
        using var content = new MemoryStream([1, 2, 3]);

        var response = await Handler.Handle(
            new UploadSavingGoalImageCommand(id, content, "image/png", 3),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal($"/api/saving-goals/{id}/image", savingGoal.ImageUrl);
        await _blobStorage.Received(1).UploadAsync(BlobContainers.SavingGoalImages, id.ToString(), content, "image/png", Arg.Any<CancellationToken>());
        await _repository.Received(1).Update(Arg.Any<CancellationToken>());
    }
}
