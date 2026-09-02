namespace FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;

public sealed class CreateDiscountCodeHandler(ICreateDiscountCodeRepository repository)
    : IRequestHandler<CreateDiscountCodeCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            StoreName = request.StoreName,
            CodeText = request.CodeText,
            CodeImageUrl = request.CodeImageUrl,
            ExpiryDate = request.ExpiryDate
        };
        var id = await repository.Add(discountCode, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
