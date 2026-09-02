namespace FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

public static class CreateSavingGoalEndpoint
{
    public static RouteGroupBuilder MapCreateSavingGoal(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateSavingGoalCommand command, CreateSavingGoalHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/saving-goals/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateSavingGoalCommand>>();

        return group;
    }
}
