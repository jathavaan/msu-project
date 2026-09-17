namespace FinanceOne.Api.Domain.Entites;

// "Description contains Keyword (case-insensitive)" -> CategoryId. Applied during Transaction
// import; rows matching no rule land in the Uncategorized bucket (Transaction.CategoryId == null)
// until manually overridden.
public sealed class CategorizationRule
{
    public Guid Id { get; init; }
    public required string Keyword { get; set; }

    public required Guid CategoryId { get; set; }
    public Category? Category { get; init; }
}
