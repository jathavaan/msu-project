using FinanceOne.Api.Features.Income.GetIncomeById;

namespace FinanceOne.UnitTests.Features.Income.GetIncomeById;

public class GetIncomeByIdHandlerTests
{
    private readonly IGetIncomeByIdRepository _repository = Substitute.For<IGetIncomeByIdRepository>();

    private GetIncomeByIdHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Income_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IncomeVm?)null);

        var response = await Handler.Handle(new GetIncomeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Income_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var vm = new IncomeVm(id, "Monthly Salary", 45_000m, Guid.NewGuid(), "Salary", 25);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(vm);

        var response = await Handler.Handle(new GetIncomeByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(vm, response.Result);
    }
}
