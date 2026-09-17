namespace FinanceOne.Api.Features.Transactions.ImportTransactions;

// Carries an IFormFile's already-opened stream rather than the IFormFile itself, so the handler
// (and its unit tests) don't need to depend on ASP.NET Core's HTTP model binding types.
public sealed record ImportTransactionsCommand(Stream Content, string FileName, long Length)
    : IRequest<Response<ImportTransactionsResultVm>>;
