namespace FinanceOne.Api.Features.Transactions.ImportTransactions;

public static class ImportTransactionsEndpoint
{
    public static RouteGroupBuilder MapImportTransactions(this RouteGroupBuilder group)
    {
        group.MapPost("/import", async (IFormFile file, ImportTransactionsHandler handler, CancellationToken ct) =>
            {
                await using var stream = file.OpenReadStream();
                var response = await handler.Handle(
                    new ImportTransactionsCommand(stream, file.FileName, file.Length), ct);
                return response.IsSuccess
                    ? Results.Ok(response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            // Binding IFormFile makes ASP.NET Core mark this endpoint as requiring antiforgery
            // validation by default. This API has no cookie-based session/antiforgery token flow
            // (it's called programmatically, not from a browser form), so there's nothing for that
            // validation to check — without this, every request 500s with "contains anti-forgery
            // metadata, but a middleware was not found that supports anti-forgery".
            .DisableAntiforgery();

        return group;
    }
}
