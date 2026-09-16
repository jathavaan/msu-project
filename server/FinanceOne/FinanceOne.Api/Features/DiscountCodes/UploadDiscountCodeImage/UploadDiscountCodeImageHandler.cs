using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.DiscountCodes.UploadDiscountCodeImage;

public sealed class UploadDiscountCodeImageHandler(IUploadDiscountCodeImageRepository repository, IBlobStorageService blobStorage)
    : IRequestHandler<UploadDiscountCodeImageCommand, Response<Unit>>
{
    // IFormFile-bound requests can't go through the shared FluentValidation ValidationFilter (it
    // matches TRequest against the endpoint's bound arguments, and a multipart form has no JSON
    // command to bind), so this file's business rules are checked here instead.
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public async Task<Response<Unit>> Handle(UploadDiscountCodeImageCommand request, CancellationToken cancellationToken)
    {
        var discountCode = await repository.GetById(request.Id, cancellationToken);
        if (discountCode is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Discount code not found.");
        }

        if (string.IsNullOrEmpty(request.ContentType) || !request.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Response<Unit>.Failure(StatusCodes.Status400BadRequest, "The uploaded file must be an image.");
        }

        if (request.Length is <= 0 or > MaxImageBytes)
        {
            return Response<Unit>.Failure(StatusCodes.Status400BadRequest, "The image must be no larger than 5 MB.");
        }

        await blobStorage.UploadAsync(BlobContainers.Coupons, request.Id.ToString(), request.Content, request.ContentType, cancellationToken);

        discountCode.CodeImageUrl = $"/api/discount-codes/{request.Id}/image";
        await repository.Update(cancellationToken);

        return Response<Unit>.Success(new Unit());
    }
}
