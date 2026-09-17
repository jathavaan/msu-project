using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;
using FinanceOne.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Features.SavingGoals.DeleteSavingGoal;

public class DeleteSavingGoalTests(MySqlFixture fixture, AzuriteFixture azuriteFixture) : IntegrationTest(fixture)
{
    private DeleteSavingGoalHandler Handler => new(new DeleteSavingGoalRepository(Context), azuriteFixture.BlobStorageService);

    [Fact]
    public async Task Returns_404_When_The_Goal_Does_Not_Exist()
    {
        var response = await Handler.Handle(new DeleteSavingGoalCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Removes_An_Unreferenced_Goal()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        var response = await Handler.Handle(new DeleteSavingGoalCommand(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);

        await using var context = NewContext();
        Assert.False(await context.SavingGoals.AnyAsync(s => s.Id == goal.Id));
    }

    // MonthlySaving -> SavingGoal is OnDelete(Restrict), so without the guard MySQL would reject
    // the DELETE and the slice would surface a 500 rather than a 409.
    [Fact]
    public async Task Returns_409_When_A_Monthly_Saving_Still_References_It()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        var response = await Handler.Handle(new DeleteSavingGoalCommand(goal.Id), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        await using var context = NewContext();
        Assert.True(await context.SavingGoals.AnyAsync(s => s.Id == goal.Id));
    }

    [Fact]
    public async Task Becomes_Deletable_Once_Its_Monthly_Savings_Are_Gone()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var monthlySaving = await GivenMonthlySaving(goal.Id, "Car Fund", 5_000m, 25);

        Context.MonthlySavings.Remove(monthlySaving);
        await Context.SaveChangesAsync();

        var response = await Handler.Handle(new DeleteSavingGoalCommand(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    // The guard looks at this goal's own references, so another goal's monthly saving must not
    // block it.
    [Fact]
    public async Task Is_Not_Blocked_By_Another_Goals_Monthly_Saving()
    {
        var car = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        var holiday = await GivenSavingGoal("Holiday", 30_000m, new DateOnly(2027, 8, 1));
        await GivenMonthlySaving(holiday.Id, "Holiday Fund", 1_500m, 5);

        var response = await Handler.Handle(new DeleteSavingGoalCommand(car.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task Deletes_The_Goals_Uploaded_Image_Too()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));
        await azuriteFixture.BlobStorageService.UploadAsync(
            BlobContainers.SavingGoalImages, goal.Id.ToString(), new MemoryStream([1]), "image/png", CancellationToken.None);

        var response = await Handler.Handle(new DeleteSavingGoalCommand(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Null(await azuriteFixture.BlobStorageService.DownloadAsync(
            BlobContainers.SavingGoalImages, goal.Id.ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task Deleting_A_Goal_With_No_Image_Does_Not_Fail()
    {
        var goal = await GivenSavingGoal("New Car", 250_000m, new DateOnly(2027, 6, 15));

        var response = await Handler.Handle(new DeleteSavingGoalCommand(goal.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
