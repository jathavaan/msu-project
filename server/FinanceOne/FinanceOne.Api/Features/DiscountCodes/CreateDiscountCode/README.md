# Add Discount Code

**Endpoint:** `POST /api/discount-codes`

Stores a discount code (text and/or image) with its expiry date.

**Behavior**
- Accepts a store/label name, the code text and/or an image, and an expiry date.
- Returns 400 if the expiry date is in the past.
- On success, persists the code and returns its generated id.

**Open question**
- Image storage isn't modeled anywhere yet (no file/blob storage decided for this project). Needs a decision — store as a byte blob in the DB, or externally (e.g. blob storage) with just a reference stored here — before this can be implemented.
