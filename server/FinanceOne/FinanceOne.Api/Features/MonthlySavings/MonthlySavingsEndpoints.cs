using FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;
using FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;
using FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;
using FinanceOne.Api.Features.MonthlySavings.GetMonthlySavings;
using FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

namespace FinanceOne.Api.Features.MonthlySavings;

public static class MonthlySavingsEndpoints
{
    public static void MapMonthlySavingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/monthly-savings").WithTags("MonthlySavings");

        group.MapCreateMonthlySaving();
        group.MapGetMonthlySavings();
        group.MapGetMonthlySavingById();
        group.MapUpdateMonthlySaving();
        group.MapDeleteMonthlySaving();
    }
}
