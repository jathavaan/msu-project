# Add Discount Code

**Endpoint:** `POST /api/discount-codes`

Stores a discount code (text and/or image) with its expiry date.

**Behavior**
- Accepts a store/label name, the code text and/or an image, and an expiry date.
- Returns 400 if the expiry date is in the past.
- On success, persists the code and returns its generated id.
- Image storage: `CodeImageUrl` stores a reference to the image, not the image bytes itself. It can
  be an arbitrary external URL passed in here, or, once the discount code exists, an image can be
  uploaded through the API itself via Upload Discount Code Image, which overwrites `CodeImageUrl`
  with `/api/discount-codes/{id}/image` and serves it back through Get Discount Code Image. Either
  way, this slice itself never touches Blob Storage — it just persists whatever string it's given.
