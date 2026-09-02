using FinanceOne.Api.Features.Budgets.CreateBudget;
using FinanceOne.Api.Features.Budgets.DeleteBudget;
using FinanceOne.Api.Features.Budgets.GetBudgetById;
using FinanceOne.Api.Features.Budgets.GetBudgets;
using FinanceOne.Api.Features.Budgets.UpdateBudget;

namespace FinanceOne.Api.Features.Budgets;

public static class BudgetsEndpoints
{
    public static void MapBudgetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapCreateBudget();
        group.MapGetBudgets();
        group.MapGetBudgetById();
        group.MapUpdateBudget();
        group.MapDeleteBudget();
    }
}
