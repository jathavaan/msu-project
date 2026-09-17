# Update Transaction Category

**Endpoint:** `PUT /api/transactions/{id}/category`

Manually assigns (or clears) a transaction's category — the override path for rows
`ImportTransactions` left in the Uncategorized bucket, or to correct a wrong keyword-rule match.

**Behavior**
- Body is `{ "id": "<same as route>", "categoryId": "<guid>" | null }`.
- Returns 400 Bad Request if the route id and body id don't match.
- Returns 404 Not Found if the transaction doesn't exist.
- Returns 404 Not Found if `categoryId` is non-null and doesn't reference an existing category.
- `categoryId: null` moves the transaction back to Uncategorized.
- On success, returns 204 No Content.
