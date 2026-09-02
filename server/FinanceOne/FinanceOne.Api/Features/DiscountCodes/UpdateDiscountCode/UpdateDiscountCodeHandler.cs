namespace FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

public sealed class UpdateDiscountCodeHandler(IUpdateDiscountCodeRepository repository)
    : IRequestHandler<UpdateDiscountCodeCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        var discountCode = await repository.GetById(request.Id, cancellationToken);
        if (discountCode is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Discount code not found.");
        }

        discountCode.StoreName = request.StoreName;
        discountCode.CodeText = request.CodeText;
        discountCode.CodeImageUrl = request.CodeImageUrl;
        discountCode.ExpiryDate = request.ExpiryDate;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
