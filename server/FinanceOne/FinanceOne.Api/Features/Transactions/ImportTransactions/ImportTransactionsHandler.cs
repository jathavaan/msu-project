using System.Security.Cryptography;
using System.Text;
using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.Transactions.ImportTransactions;

public sealed class ImportTransactionsHandler(IImportTransactionsRepository repository, IBlobStorageService blobStorage)
    : IRequestHandler<ImportTransactionsCommand, Response<ImportTransactionsResultVm>>
{
    // IFormFile-bound requests can't go through the shared FluentValidation ValidationFilter (it
    // matches TRequest against the endpoint's bound arguments, and a multipart form has no JSON
    // command to bind), so this slice's business rules are checked here instead — same reasoning
    // as UploadDiscountCodeImageHandler.
    private const long MaxCsvBytes = 10 * 1024 * 1024;

    public async Task<Response<ImportTransactionsResultVm>> Handle(ImportTransactionsCommand request, CancellationToken cancellationToken)
    {
        if (!request.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Response<ImportTransactionsResultVm>.Failure(
                StatusCodes.Status400BadRequest, "The uploaded file must be a .csv export.");
        }

        if (request.Length is <= 0 or > MaxCsvBytes)
        {
            return Response<ImportTransactionsResultVm>.Failure(
                StatusCodes.Status400BadRequest, "The file must be non-empty and no larger than 10 MB.");
        }

        // Buffered so the exact bytes can both be staged to Blob Storage (for audit/reprocessing)
        // and then parsed, without requiring the HTTP request stream to support seeking.
        using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);

        buffer.Position = 0;
        await blobStorage.UploadAsync(
            BlobContainers.StagedCsv, $"{Guid.NewGuid()}-{request.FileName}", buffer, "text/csv", cancellationToken);

        buffer.Position = 0;
        ParsedCsv parsed;
        try
        {
            parsed = TransaksjonslisteCsvParser.Parse(buffer);
        }
        catch (Exception)
        {
            return Response<ImportTransactionsResultVm>.Failure(
                StatusCodes.Status400BadRequest, "The file isn't a valid Transaksjonsliste CSV export.");
        }

        var rules = await repository.GetCategorizationRules(cancellationToken);

        var candidateHashes = parsed.Rows.Select(ComputeHash).Distinct().ToList();
        var existingHashes = await repository.GetExistingHashes(candidateHashes, cancellationToken);

        var toInsert = new List<Transaction>();
        var seenInBatch = new HashSet<string>();
        var duplicateSkipped = 0;
        var uncategorized = 0;

        foreach (var row in parsed.Rows)
        {
            var hash = ComputeHash(row);
            if (existingHashes.Contains(hash) || !seenInBatch.Add(hash))
            {
                duplicateSkipped++;
                continue;
            }

            var categoryId = MatchCategory(row.Description, rules);
            if (categoryId is null)
            {
                uncategorized++;
            }

            toInsert.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                Date = row.Date,
                Description = row.Description,
                Amount = row.Amount,
                CategoryId = categoryId,
                Hash = hash
            });
        }

        await repository.AddRange(toInsert, cancellationToken);

        return Response<ImportTransactionsResultVm>.Success(new ImportTransactionsResultVm(
            toInsert.Count,
            duplicateSkipped,
            parsed.PendingSkippedCount,
            parsed.TransferExcludedCount,
            parsed.InvalidRowCount,
            uncategorized));
    }

    private static string ComputeHash(ParsedTransactionRow row) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{row.Date:O}|{row.Amount}|{row.Description}")));

    // Longest-keyword-first so a more specific rule (e.g. "REMA 1000") wins over a broader one
    // (e.g. "REMA") when both match the same description.
    private static Guid? MatchCategory(string description, List<CategorizationRule> rules) =>
        rules
            .OrderByDescending(r => r.Keyword.Length)
            .FirstOrDefault(r => description.Contains(r.Keyword, StringComparison.OrdinalIgnoreCase))
            ?.CategoryId;
}
