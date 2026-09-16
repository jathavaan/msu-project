# Get Discount Code Image

**Endpoint:** `GET /api/discount-codes/{id}/image`

Streams back the image uploaded for a discount code (see Upload Discount Code Image). The API
proxies the download from Blob Storage — the response is the raw image bytes with the original
`Content-Type`, not a redirect or a SAS URL.

**Behavior**
- Returns 404 Not Found if the discount code doesn't exist.
- Returns 404 Not Found if no image has been uploaded for it.
