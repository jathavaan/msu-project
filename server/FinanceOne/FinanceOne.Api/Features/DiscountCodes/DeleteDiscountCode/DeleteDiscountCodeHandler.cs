using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;

public sealed class DeleteDiscountCodeHandler(IDeleteDiscountCodeRepository repository, IBlobStorageService blobStorage)
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

        // Images are kept indefinitely while the discount code exists (no expiry-driven cleanup),
        // but deleting the code itself should not leave an orphaned blob behind. No-op if the code
        // never had an image uploaded through Upload Discount Code Image.
        await blobStorage.DeleteAsync(BlobContainers.Coupons, request.Id.ToString(), cancellationToken);

        return Response<Unit>.Success(new Unit());
    }
}
