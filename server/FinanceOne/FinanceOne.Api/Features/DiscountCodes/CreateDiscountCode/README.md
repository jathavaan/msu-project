# Add Discount Code

**Endpoint:** `POST /api/discount-codes`

Stores a discount code (text and/or image) with its expiry date.

**Behavior**
- Accepts a store/label name, the code text, and an expiry date.
- Returns 400 if the expiry date is in the past.
- On success, persists the code and returns its generated id.
- Image storage: this slice never sets `CodeImageUrl` — a code has no image until one is uploaded,
  once the discount code exists, through Upload Discount Code Image. That slice writes
  `/api/discount-codes/{id}/image` (served back through Get Discount Code Image) and is the only
  way to set an image; there is no way to point a code at an arbitrary external image URL.
