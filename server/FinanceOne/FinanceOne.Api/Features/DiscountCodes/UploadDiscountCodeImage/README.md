# Upload Discount Code Image

**Endpoint:** `PUT /api/discount-codes/{id}/image` (multipart form, field name `file`)

Uploads a photo/screenshot of the coupon and stores it in Blob Storage, proxied through the API —
the client never talks to Blob Storage directly, and no SAS token is issued.

**Behavior**
- Returns 404 Not Found if the discount code doesn't exist.
- Returns 400 Bad Request if the file isn't an image (`Content-Type` doesn't start with `image/`)
  or is empty/larger than 5 MB.
- On success, the file replaces any previously uploaded image for this discount code (same blob
  name — the discount code's id) and `CodeImageUrl` is set to `/api/discount-codes/{id}/image`
  (see Get Discount Code Image), overwriting whatever `CodeImageUrl` held before.
- The image is kept indefinitely — it is not deleted when the discount code expires, only when the
  discount code itself is deleted (see Delete Discount Code).
