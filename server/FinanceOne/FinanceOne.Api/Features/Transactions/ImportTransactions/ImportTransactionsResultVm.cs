namespace FinanceOne.Api.Features.Transactions.ImportTransactions;

public sealed record ImportTransactionsResultVm(
    int ImportedCount,
    int DuplicateSkippedCount,
    int PendingSkippedCount,
    int TransferExcludedCount,
    int InvalidRowCount,
    int UncategorizedCount);
