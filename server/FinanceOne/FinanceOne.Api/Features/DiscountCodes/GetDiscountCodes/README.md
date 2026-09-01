# List Discount Codes

**Endpoint:** `GET /api/discount-codes?expiringWithinDays=`

Returns stored discount codes, optionally filtered to ones expiring soon.

**Behavior**
- `expiringWithinDays` query parameter is optional; when omitted, returns all codes (including already-expired ones, so the user can clean them up).
- Returns an empty list if none exist.
