# Add Discount Code

**Endpoint:** `POST /api/discount-codes`

Stores a discount code (text and/or image) with its expiry date.

**Behavior**
- Accepts a store/label name, the code text and/or an image, and an expiry date.
- Returns 400 if the expiry date is in the past.
- On success, persists the code and returns its generated id.
- Image storage: `CodeImageUrl` stores a reference (URL) to the image, not the image bytes itself — the caller is responsible for uploading the image elsewhere and passing the resulting URL. No blob storage is modeled in this API.
