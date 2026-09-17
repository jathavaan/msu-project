namespace FinanceOne.Api.Domain.Entites;

// A real, dated ledger entry from an imported bank statement — distinct from the planned recurring
// Income/Expense tables. Positive Amount is money in, negative is money out.
public sealed class Transaction
{
    public Guid Id { get; init; }
    public required DateOnly Date { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }

    // Null means "Uncategorized" — no CategorizationRule matched and no manual override was set yet.
    public Guid? CategoryId { get; set; }
    public Category? Category { get; init; }

    // SHA-256 of Date|Amount|Description, unique per row. Lets a re-imported file skip rows it
    // already brought in without a full-table row comparison.
    public required string Hash { get; set; }
}
