using FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;

namespace FinanceOne.UnitTests.Features.MonthlySavings.GetMonthlySavingById;

public class GetMonthlySavingByIdHandlerTests
{
    private readonly IGetMonthlySavingByIdRepository _repository =
        Substitute.For<IGetMonthlySavingByIdRepository>();

    private GetMonthlySavingByIdHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Monthly_Saving_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MonthlySavingVm?)null);

        var response = await Handler.Handle(new GetMonthlySavingByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    [Fact]
    public async Task Returns_The_Monthly_Saving_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var vm = new MonthlySavingVm(id, "Car Fund", 5_000m, Guid.NewGuid(), "New Car", 25);
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(vm);

        var response = await Handler.Handle(new GetMonthlySavingByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(vm, response.Result);
    }
}
