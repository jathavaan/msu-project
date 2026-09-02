namespace FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;

public sealed class DeleteDiscountCodeHandler(IDeleteDiscountCodeRepository repository)
    : IRequestHandler<DeleteDiscountCodeCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        var discountCode = await repository.GetById(request.Id, cancellationToken);
        if (discountCode is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Discount code not found.");
        }

        await repository.Delete(discountCode, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
