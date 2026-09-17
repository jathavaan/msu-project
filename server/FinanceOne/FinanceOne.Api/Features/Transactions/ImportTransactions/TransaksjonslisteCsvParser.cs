using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace FinanceOne.Api.Features.Transactions.ImportTransactions;

// A row surviving Bokført/transfer filtering, ready to be hashed/categorized/persisted.
internal sealed record ParsedTransactionRow(DateOnly Date, string Description, decimal Amount);

internal sealed record ParsedCsv(
    List<ParsedTransactionRow> Rows,
    int PendingSkippedCount,
    int TransferExcludedCount,
    int InvalidRowCount);

/// <summary>
/// Parses the semicolon-delimited "Transaksjonsliste" CSV export (see issue #47) — Norwegian
/// headers, RFC4180 quoting (CsvHelper handles a `Beskrivelse` containing embedded newlines
/// automatically as long as it's quoted), amount split across two columns.
///
/// Expected header row: <c>Dato;Beskrivelse;Type;Undertype;Beløp inn;Beløp ut;Status</c>. A
/// generic user-facing column-mapping UI is deferred until a second bank format is actually
/// needed (see the issue) — this parser is intentionally tied to this one layout.
/// </summary>
internal static class TransaksjonslisteCsvParser
{
    private const string DateColumn = "Dato";
    private const string DescriptionColumn = "Beskrivelse";
    private const string TypeColumn = "Type";
    private const string SubtypeColumn = "Undertype";
    private const string AmountInColumn = "Beløp inn";
    private const string AmountOutColumn = "Beløp ut";
    private const string StatusColumn = "Status";

    private const string BookedStatus = "Bokført";
    private const string TransferType = "Overføring";
    private const string OwnAccountMarker = "egen konto";

    public static ParsedCsv Parse(Stream csv)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            // A row missing/mismatching a column is treated as invalid data (see InvalidRowCount)
            // rather than a hard parse failure of the whole file.
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(csv, leaveOpen: true);
        using var csvReader = new CsvReader(reader, config);

        csvReader.Read();
        csvReader.ReadHeader();

        var rows = new List<ParsedTransactionRow>();
        var pendingSkipped = 0;
        var transferExcluded = 0;
        var invalid = 0;

        while (csvReader.Read())
        {
            var status = csvReader.GetField(StatusColumn)?.Trim();
            if (!string.Equals(status, BookedStatus, StringComparison.OrdinalIgnoreCase))
            {
                pendingSkipped++;
                continue;
            }

            var type = csvReader.GetField(TypeColumn)?.Trim() ?? string.Empty;
            var subtype = csvReader.GetField(SubtypeColumn)?.Trim() ?? string.Empty;
            if (IsOwnAccountTransfer(type, subtype))
            {
                transferExcluded++;
                continue;
            }

            var description = csvReader.GetField(DescriptionColumn)?.Trim();
            if (string.IsNullOrEmpty(description)
                || !TryParseDate(csvReader.GetField(DateColumn), out var date)
                || !TryResolveAmount(csvReader.GetField(AmountInColumn), csvReader.GetField(AmountOutColumn), out var amount))
            {
                invalid++;
                continue;
            }

            rows.Add(new ParsedTransactionRow(date, description, amount));
        }

        return new ParsedCsv(rows, pendingSkipped, transferExcluded, invalid);
    }

    // Rows moving money between the user's own accounts aren't real income/spending and would
    // double-count money that just moved — see issue #47.
    private static bool IsOwnAccountTransfer(string type, string subtype) =>
        type.Contains(TransferType, StringComparison.OrdinalIgnoreCase)
        && subtype.Contains(OwnAccountMarker, StringComparison.OrdinalIgnoreCase);

    private static bool TryParseDate(string? raw, out DateOnly date) =>
        DateOnly.TryParseExact(raw?.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    // Exactly one of Beløp inn/Beløp ut is populated per row. Beløp inn is a credit (money in,
    // positive); Beløp ut is a debit (money out, stored as negative) — regardless of whether this
    // particular export already signs it negative itself (see TryParseAmount below), -Math.Abs
    // normalizes it either way instead of assuming one sign convention over the other.
    private static bool TryResolveAmount(string? amountIn, string? amountOut, out decimal amount)
    {
        var hasIn = !string.IsNullOrWhiteSpace(amountIn);
        var hasOut = !string.IsNullOrWhiteSpace(amountOut);

        if (hasIn == hasOut)
        {
            amount = 0m;
            return false;
        }

        if (!TryParseAmount(hasIn ? amountIn : amountOut, out var value))
        {
            amount = 0m;
            return false;
        }

        amount = hasIn ? value : -Math.Abs(value);
        return true;
    }

    // Real exports of this same bank feature have shown up in two different number formats: comma
    // decimal with an optional "." thousands separator and an unsigned Beløp ut magnitude (e.g.
    // "1.500,00"), and plain period-decimal with Beløp ut already signed negative (e.g. "-568.65",
    // seen on an English-locale export — see issue #96). Parsing against
    // CultureInfo.GetCultureInfo("nb-NO") handles neither: its NumberGroupSeparator is U+00A0 (a
    // non-breaking space), not ".", so it rejects the first format outright, and naively stripping
    // "." to work around that corrupts the second format's decimal point instead (e.g. "-568.65"
    // -> "-56865"). Whichever of "." / "," appears *last* is the actual decimal separator — a
    // thousands separator, when present at all, always appears before it — so that's used to
    // decide which one to strip rather than assuming either format up front.
    private static bool TryParseAmount(string? raw, out decimal value)
    {
        if (raw is null)
        {
            value = 0m;
            return false;
        }

        var trimmed = raw.Trim();
        var lastDot = trimmed.LastIndexOf('.');
        var lastComma = trimmed.LastIndexOf(',');

        var normalized = lastComma > lastDot
            ? trimmed.Replace(".", string.Empty).Replace(',', '.')
            : trimmed.Replace(",", string.Empty);

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
