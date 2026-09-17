namespace FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;

public static class CreateCategorizationRuleEndpoint
{
    public static RouteGroupBuilder MapCreateCategorizationRule(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateCategorizationRuleCommand command, CreateCategorizationRuleHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/categorization-rules/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateCategorizationRuleCommand>>();

        return group;
    }
}
