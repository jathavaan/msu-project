# List Transactions

**Endpoint:** `GET /api/transactions?categoryId=&uncategorizedOnly=&from=&to=`

Returns imported bank transactions (actuals), most recent first — distinct from the planned
recurring `Income`/`Expenses` lists.

**Behavior**
- All query parameters are optional; omitting all of them returns every transaction.
- `categoryId` filters to one category.
- `uncategorizedOnly=true` filters to transactions with no assigned category (the bucket
  `ImportTransactions` leaves rows in when no `CategorizationRule` matched) — combine with
  `UpdateTransactionCategory` to work through them.
- `from`/`to` filter by (inclusive) transaction date.
- Returns an empty list if nothing matches.
