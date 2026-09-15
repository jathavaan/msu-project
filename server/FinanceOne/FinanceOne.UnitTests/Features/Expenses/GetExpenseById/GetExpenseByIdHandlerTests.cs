using FinanceOne.Api.Features.Expenses.GetExpenseById;

namespace FinanceOne.UnitTests.Features.Expenses.GetExpenseById;

public class GetExpenseByIdHandlerTests
{
    private readonly IGetExpenseByIdRepository _repository = Substitute.For<IGetExpenseByIdRepository>();

    private GetExpenseByIdHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Expense_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ExpenseVm?)null);

        var response = await Handler.Handle(new GetExpenseByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Expense_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var vm = new ExpenseVm(id, "Monthly Rent", 12_000m, Guid.NewGuid(), "Rent", 1);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(vm);

        var response = await Handler.Handle(new GetExpenseByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(vm, response.Result);
    }
}
