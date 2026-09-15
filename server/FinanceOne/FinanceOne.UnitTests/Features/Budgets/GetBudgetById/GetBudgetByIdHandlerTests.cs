using FinanceOne.Api.Features.Budgets.GetBudgetById;

namespace FinanceOne.UnitTests.Features.Budgets.GetBudgetById;

public class GetBudgetByIdHandlerTests
{
    private readonly IGetBudgetByIdRepository _repository = Substitute.For<IGetBudgetByIdRepository>();

    private GetBudgetByIdHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_Budget_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((BudgetVm?)null);

        var response = await Handler.Handle(new GetBudgetByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Budget_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var vm = new BudgetVm(id, Guid.NewGuid(), "Rent", 12_000m, 8_000m);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(vm);

        var response = await Handler.Handle(new GetBudgetByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(vm, response.Result);
    }
}
