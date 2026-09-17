namespace FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;

// CategoryId == null moves the transaction back to the Uncategorized bucket.
public sealed record UpdateTransactionCategoryCommand(Guid Id, Guid? CategoryId) : IRequest<Response<Unit>>;
