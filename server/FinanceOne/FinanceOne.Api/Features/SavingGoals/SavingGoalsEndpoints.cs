using FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;
using FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalById;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalProjection;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoals;
using FinanceOne.Api.Features.SavingGoals.GetSavingGoalsProjection;
using FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;
using FinanceOne.Api.Features.SavingGoals.UploadSavingGoalImage;

namespace FinanceOne.Api.Features.SavingGoals;

public static class SavingGoalsEndpoints
{
    public static void MapSavingGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/saving-goals").WithTags("SavingGoals");

        group.MapCreateSavingGoal();
        group.MapGetSavingGoals();
        group.MapGetSavingGoalById();
        group.MapGetSavingGoalProjection();
        group.MapGetSavingGoalsProjection();
        group.MapUpdateSavingGoal();
        group.MapDeleteSavingGoal();
        group.MapUploadSavingGoalImage();
        group.MapGetSavingGoalImage();
    }
}
