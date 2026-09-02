using FinanceOne.Api.Features.Income.CreateIncome;
using FinanceOne.Api.Features.Income.DeleteIncome;
using FinanceOne.Api.Features.Income.GetIncomeById;
using FinanceOne.Api.Features.Income.GetIncomes;
using FinanceOne.Api.Features.Income.UpdateIncome;

namespace FinanceOne.Api.Features.Income;

public static class IncomeEndpoints
{
    public static void MapIncomeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/income").WithTags("Income");

        group.MapCreateIncome();
        group.MapGetIncomes();
        group.MapGetIncomeById();
        group.MapUpdateIncome();
        group.MapDeleteIncome();
    }
}
