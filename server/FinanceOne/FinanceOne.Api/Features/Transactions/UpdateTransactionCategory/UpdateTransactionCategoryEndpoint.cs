namespace FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;

public static class UpdateTransactionCategoryEndpoint
{
    public static RouteGroupBuilder MapUpdateTransactionCategory(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}/category", async (Guid id, UpdateTransactionCategoryCommand command, UpdateTransactionCategoryHandler handler, CancellationToken ct) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest();
            }

            var response = await handler.Handle(command, ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
