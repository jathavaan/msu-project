# Import Transactions

**Endpoint:** `POST /api/transactions/import` (multipart form, field name `file`)

Imports a bank's "Transaksjonsliste" CSV export as a batch of `Transaction` rows — a separate
ledger of actuals, distinct from the planned recurring `Income`/`Expenses` tables (see issue #47).

**Expected file format**

Semicolon-delimited, RFC4180-quoted, with this header row:

```
Dato;Beskrivelse;Type;Undertype;Beløp inn;Beløp ut;Status
```

| Column | Meaning |
|---|---|
| `Dato` | Transaction date, `dd.MM.yyyy` |
| `Beskrivelse` | Free-text description/message. May be quoted and contain embedded newlines — parsed via CsvHelper, not a hand-rolled splitter, specifically to handle that. |
| `Type` | Transaction type, e.g. `Varekjøp`, `Overføring` |
| `Undertype` | Transaction subtype, e.g. `Til egen konto` |
| `Beløp inn` | Credit amount (money in), Norwegian decimal formatting (`,` decimal separator). Empty when the row is a debit. |
| `Beløp ut` | Debit amount (money out), same formatting. Empty when the row is a credit. |
| `Status` | `Bokført` (booked/final) or `Reservert` (pending) |

This is the one bank format this parser understands. A generic user-facing column-mapping UI is
deferred until a second bank format is actually needed — see issue #47.

**Behavior**

- Returns 400 Bad Request if the file isn't named `*.csv`, is empty, is larger than 10 MB, or
  fails to parse as this format at all.
- The raw uploaded bytes are staged in Blob Storage (`staged-csv` container, named
  `{guid}-{original filename}`) before anything is parsed or persisted, for audit/reprocessing —
  this happens even if parsing subsequently fails.
- Only rows with `Status = Bokført` are considered; `Reservert` rows are skipped and picked up
  naturally on a later import once booked (counted as `pendingSkippedCount`).
- A row is treated as a transfer between the user's own accounts, and excluded, when `Type`
  contains "Overføring" (case-insensitive) **and** `Undertype` contains "egen konto"
  (case-insensitive) — counted as `transferExcludedCount`.
- A row missing its description/date, or with neither/both of `Beløp inn`/`Beløp ut` populated, or
  with an unparsable date/amount, is skipped and counted as `invalidRowCount`.
- Amount is `+Beløp inn` or `-Beløp ut` — whichever of the two is populated.
- **Dedup:** each surviving row is hashed (`SHA-256` of `Date|Amount|Description`). A row whose
  hash already exists in the `Transactions` table (from a previous import) or appears more than
  once within the same file is skipped and counted as `duplicateSkippedCount` — this is what makes
  re-importing the same export (or an export with overlapping date ranges) safe to run again.
- **Categorization:** each remaining row is matched against every `CategorizationRule` (see the
  CategorizationRules feature group) — the longest matching keyword wins when more than one rule's
  keyword appears in the description (case-insensitive substring match). A row matching no rule is
  imported with `CategoryId = null` (the "Uncategorized" bucket) rather than being rejected;
  `UpdateTransactionCategory` covers the manual override. Counted as `uncategorizedCount`.
- On success, returns a summary: `importedCount`, `duplicateSkippedCount`, `pendingSkippedCount`,
  `transferExcludedCount`, `invalidRowCount`, `uncategorizedCount`.
- Processing happens synchronously within the request, right after staging — there's no background
  import job/queue in this codebase yet, so "staged before processed" is satisfied by ordering
  rather than by decoupling the two onto separate infrastructure.
