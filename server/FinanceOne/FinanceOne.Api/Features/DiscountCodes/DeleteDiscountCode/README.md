# Delete Discount Code

**Endpoint:** `DELETE /api/discount-codes/{id}`

Removes a stored discount code.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Also deletes any image uploaded for it (see Upload Discount Code Image), so no blob is left
  behind once the discount code itself is gone.
