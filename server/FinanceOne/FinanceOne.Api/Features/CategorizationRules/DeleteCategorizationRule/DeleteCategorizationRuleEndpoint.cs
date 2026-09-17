namespace FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;

public static class DeleteCategorizationRuleEndpoint
{
    public static RouteGroupBuilder MapDeleteCategorizationRule(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteCategorizationRuleHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteCategorizationRuleCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
