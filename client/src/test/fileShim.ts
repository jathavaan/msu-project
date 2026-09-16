/**
 * jsdom supplies its own `File`/`Blob`/`FormData`, but `Request`/`fetch` stay Node's (undici) —
 * same situation as abortSignalShim.ts, just for multipart file uploads instead of cancellation
 * (see SavingGoalForm's image upload tests). MSW constructs its own `Request` directly from
 * `fetch`'s arguments to find a matching handler — patching `fetch` itself runs too late, since a
 * matched request never reaches the wrapped function at all. undici's `Request` then brand-checks
 * every `FormData` entry it serializes (`webidl.is.File(value)`) against *its own* `File` class and
 * throws on jsdom's, which is what `formData.append('file', file)` stores when `file` came from a
 * `<input type="file">` change event — jsdom's own `FormData.append` requires a jsdom `Blob` to
 * store the value as a file rather than silently stringifying it, so swapping the globals (as
 * abortSignalShim does for `AbortSignal`) isn't an option without breaking that.
 *
 * Instead, `Request` is patched (layered on top of abortSignalShim's, which already ran) so that a
 * FormData body is pre-encoded into a raw multipart byte stream, with the boundary set on
 * `Content-Type`, before undici ever sees a jsdom File/Blob. This reproduces exactly what a real
 * browser sends on the wire, readable via `request.text()`/`.arrayBuffer()` in a test's handler.
 * `request.formData()` on the receiving end hits a separate, unrelated assertion failure inside
 * this environment's bundled undici when re-parsing a body — a test needing to inspect an upload's
 * contents reads the raw multipart text instead (see SavingGoalForm's image upload tests).
 *
 * Imported for its side effect at the top of setup.ts, before MSW wraps `fetch`.
 */
const NativeRequest = globalThis.Request

function randomBoundary(): string {
  return `FinanceOneTestBoundary${Math.random().toString(16).slice(2)}`
}

function encodeMultipartStream(formData: FormData, boundary: string): ReadableStream<Uint8Array> {
  const encoder = new TextEncoder()
  const entries = [...formData.entries()]
  let index = 0

  return new ReadableStream({
    async pull(controller) {
      if (index >= entries.length) {
        controller.enqueue(encoder.encode(`--${boundary}--\r\n`))
        controller.close()
        return
      }

      const [key, value] = entries[index++]
      if (value instanceof Blob) {
        const filename = 'name' in value ? (value as File).name : 'blob'
        controller.enqueue(
          encoder.encode(
            `--${boundary}\r\nContent-Disposition: form-data; name="${key}"; filename="${filename}"\r\n` +
              `Content-Type: ${value.type || 'application/octet-stream'}\r\n\r\n`,
          ),
        )
        controller.enqueue(new Uint8Array(await value.arrayBuffer()))
        controller.enqueue(encoder.encode('\r\n'))
      } else {
        controller.enqueue(
          encoder.encode(`--${boundary}\r\nContent-Disposition: form-data; name="${key}"\r\n\r\n${value}\r\n`),
        )
      }
    },
  })
}

class PatchedRequest extends NativeRequest {
  constructor(input: RequestInfo | URL, init?: RequestInit) {
    if (init?.body instanceof FormData) {
      const boundary = randomBoundary()
      const headers = new Headers(init.headers)
      headers.set('Content-Type', `multipart/form-data; boundary=${boundary}`)
      super(input, {
        ...init,
        body: encodeMultipartStream(init.body, boundary),
        headers,
        duplex: 'half',
      } as RequestInit)
    } else {
      super(input, init)
    }
  }
}

globalThis.Request = PatchedRequest as typeof Request
