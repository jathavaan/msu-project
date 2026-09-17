# Update Discount Code

**Endpoint:** `PUT /api/discount-codes/{id}`

Edits a stored discount code's label, code, or expiry date.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Same validation as Add Discount Code.
- Does not touch `CodeImageUrl` — the image, if any, is left as-is. It only changes through Upload
  Discount Code Image.
