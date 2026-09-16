# Upload Saving Goal Image

**Endpoint:** `PUT /api/saving-goals/{id}/image` (multipart form, field name `file`)

Uploads a photo of what the goal is for (e.g. the car, the trip, the purchase) and stores it in
Blob Storage, proxied through the API — the client never talks to Blob Storage directly, and no SAS
token is issued. Mirrors Upload Discount Code Image, but in its own `saving-goal-images` container.

**Behavior**
- Returns 404 Not Found if the saving goal doesn't exist.
- Returns 400 Bad Request if the file isn't an image (`Content-Type` doesn't start with `image/`)
  or is empty/larger than 5 MB.
- On success, the file replaces any previously uploaded image for this saving goal (same blob
  name — the goal's id) and `ImageUrl` is set to `/api/saving-goals/{id}/image` (see Get Saving
  Goal Image), overwriting whatever `ImageUrl` held before.
- The image is kept indefinitely — it is only deleted when the saving goal itself is deleted (see
  Delete Saving Goal).
