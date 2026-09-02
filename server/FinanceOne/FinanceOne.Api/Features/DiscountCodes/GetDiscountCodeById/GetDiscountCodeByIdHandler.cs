namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;

public sealed class GetDiscountCodeByIdHandler(IGetDiscountCodeByIdRepository repository)
    : IRequestHandler<GetDiscountCodeByIdQuery, Response<DiscountCode>>
{
    public async Task<Response<DiscountCode>> Handle(GetDiscountCodeByIdQuery request, CancellationToken cancellationToken)
    {
        var discountCode = await repository.GetById(request.Id, cancellationToken);
        return discountCode is null
            ? Response<DiscountCode>.Failure(StatusCodes.Status404NotFound, "Discount code not found.")
            : Response<DiscountCode>.Success(discountCode);
    }
}
