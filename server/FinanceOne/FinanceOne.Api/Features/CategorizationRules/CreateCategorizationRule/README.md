# Create Categorization Rule

**Endpoint:** `POST /api/categorization-rules`

Adds a keyword rule used to auto-categorize imported transactions (see Transactions'
`ImportTransactions`), e.g. "description contains 'REMA' -> Food".

**Behavior**
- Accepts a keyword and a category id. The category may be either an `Income` or `Expense`
  category — rules aren't restricted to one type, since imported transactions can be either.
- Returns 404 Not Found if the referenced category doesn't exist.
- On success, persists the rule and returns its generated id.
- Matching against a transaction's description happens at import time, case-insensitively, as a
  substring match — see `ImportTransactions/README.md`.
