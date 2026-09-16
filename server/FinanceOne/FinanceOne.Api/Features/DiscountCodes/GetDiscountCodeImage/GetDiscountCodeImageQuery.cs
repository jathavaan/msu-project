using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeImage;

public sealed record GetDiscountCodeImageQuery(Guid Id) : IRequest<Response<DownloadedBlob>>;
