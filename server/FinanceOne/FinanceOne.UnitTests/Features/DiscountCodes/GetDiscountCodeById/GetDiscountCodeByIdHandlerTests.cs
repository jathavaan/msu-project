using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;

namespace FinanceOne.UnitTests.Features.DiscountCodes.GetDiscountCodeById;

public class GetDiscountCodeByIdHandlerTests
{
    private readonly IGetDiscountCodeByIdRepository _repository = Substitute.For<IGetDiscountCodeByIdRepository>();

    private GetDiscountCodeByIdHandler Handler => new(_repository);

    [Fact]
    public async Task Returns_404_When_The_Discount_Code_Does_Not_Exist()
    {
        _repository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DiscountCode?)null);

        var response = await Handler.Handle(new GetDiscountCodeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
    }

    // This slice returns the entity directly rather than a Vm — DiscountCode has no navigation
    // properties to hide and no derived fields, so there is nothing for a Vm to add.
    [Fact]
    public async Task Returns_The_Discount_Code_When_It_Exists()
    {
        var id = Guid.NewGuid();
        var discountCode = new DiscountCode
        {
            Id = id,
            StoreName = "Rema 1000",
            CodeText = "SAVE20",
            ExpiryDate = new DateOnly(2026, 12, 31),
        };
        _repository.GetById(id, Arg.Any<CancellationToken>()).Returns(discountCode);

        var response = await Handler.Handle(new GetDiscountCodeByIdQuery(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Same(discountCode, response.Result);
    }
}
