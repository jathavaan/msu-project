using FinanceOne.Api.Features.Income.GetIncomes;

namespace FinanceOne.UnitTests.Features.Income.GetIncomes;

public class GetIncomesHandlerTests
{
    private readonly IGetIncomesRepository _repository = Substitute.For<IGetIncomesRepository>();

    private GetIncomesHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_What_The_Repository_Produced()
    {
        var incomes = new List<IncomeVm>
        {
            new(Guid.NewGuid(), "Monthly Salary", 45_000m, Guid.NewGuid(), "Salary", 25),
            new(Guid.NewGuid(), "Consulting Fee", 12_000m, Guid.NewGuid(), "Freelance", 10),
        };
        _repository.GetIncomes(Arg.Any<CancellationToken>()).Returns(incomes);

        var response = await Handler.Handle(new GetIncomesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(incomes, response.Result);
    }

    // Unlike GetExpenses this query takes no filter, so an empty database is still a 200 with an
    // empty list rather than a 404.
    [Fact]
    public async Task Returns_Success_With_An_Empty_List_When_There_Are_No_Incomes()
    {
        _repository.GetIncomes(Arg.Any<CancellationToken>()).Returns([]);

        var response = await Handler.Handle(new GetIncomesQuery(), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Result!);
    }
}
