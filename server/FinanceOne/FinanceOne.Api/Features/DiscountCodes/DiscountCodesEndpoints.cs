using FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;
using FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;
using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;
using FinanceOne.Api.Features.DiscountCodes.GetDiscountCodes;
using FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

namespace FinanceOne.Api.Features.DiscountCodes;

public static class DiscountCodesEndpoints
{
    public static void MapDiscountCodesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/discount-codes").WithTags("DiscountCodes");

        group.MapCreateDiscountCode();
        group.MapGetDiscountCodes();
        group.MapGetDiscountCodeById();
        group.MapUpdateDiscountCode();
        group.MapDeleteDiscountCode();
    }
}
