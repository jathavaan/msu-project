using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeImage;

public sealed class GetDiscountCodeImageHandler(IGetDiscountCodeImageRepository repository, IBlobStorageService blobStorage)
    : IRequestHandler<GetDiscountCodeImageQuery, Response<DownloadedBlob>>
{
    public async Task<Response<DownloadedBlob>> Handle(GetDiscountCodeImageQuery request, CancellationToken cancellationToken)
    {
        var discountCode = await repository.GetById(request.Id, cancellationToken);
        if (discountCode is null)
        {
            return Response<DownloadedBlob>.Failure(StatusCodes.Status404NotFound, "Discount code not found.");
        }

        var blob = await blobStorage.DownloadAsync(BlobContainers.Coupons, request.Id.ToString(), cancellationToken);
        if (blob is null)
        {
            return Response<DownloadedBlob>.Failure(StatusCodes.Status404NotFound, "No image has been uploaded for this discount code.");
        }

        return Response<DownloadedBlob>.Success(blob);
    }
}
