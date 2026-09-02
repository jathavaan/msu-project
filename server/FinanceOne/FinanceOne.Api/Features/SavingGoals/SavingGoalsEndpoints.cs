using FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;
using FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoals;
using FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

namespace FinanceOne.Api.Features.SavingGoals;

public static class SavingGoalsEndpoints
{
    public static void MapSavingGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/saving-goals").WithTags("SavingGoals");

        group.MapCreateSavingGoal();
        group.MapGetSavingGoals();
        group.MapGetSavingGoalById();
        group.MapUpdateSavingGoal();
        group.MapDeleteSavingGoal();
    }
}
