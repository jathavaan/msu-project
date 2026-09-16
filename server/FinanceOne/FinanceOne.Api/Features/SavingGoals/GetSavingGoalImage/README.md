# Get Saving Goal Image

**Endpoint:** `GET /api/saving-goals/{id}/image`

Streams back the image uploaded for a saving goal (see Upload Saving Goal Image). The API proxies
the download from Blob Storage — the response is the raw image bytes with the original
`Content-Type`, not a redirect or a SAS URL.

**Behavior**
- Returns 404 Not Found if the saving goal doesn't exist.
- Returns 404 Not Found if no image has been uploaded for it.
