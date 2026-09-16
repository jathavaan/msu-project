# Update Discount Code

**Endpoint:** `PUT /api/discount-codes/{id}`

Edits a stored discount code's label, code, image, or expiry date.

**Behavior**
- Returns 404 Not Found if it doesn't exist.
- Same validation as Add Discount Code.
- Setting `CodeImageUrl` here to something other than `/api/discount-codes/{id}/image` doesn't
  delete a previously uploaded image's blob — it just detaches the reference. Uploading a new image
  afterward (Upload Discount Code Image) overwrites `CodeImageUrl` back to the proxied path.
