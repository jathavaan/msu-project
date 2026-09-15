/**
 * jsdom supplies its own `AbortController`/`AbortSignal`, but not `fetch`/`Request` — those stay
 * Node's (undici). undici brand-checks `RequestInit.signal` against *its* `AbortSignal` class and
 * throws `Expected signal to be an instance of AbortSignal` on jsdom's, which is what RTK Query
 * hands it for every request it can cancel. There is no way to reach Node's class from inside the
 * jsdom global, so instead the signal is dropped at the boundary and its one observable effect —
 * the request rejecting when aborted — is reproduced here.
 *
 * Only in-flight cancellation of the underlying socket is lost, which nothing in a test observes.
 * Imported for its side effect at the very top of setup.ts, before MSW wraps these globals.
 */
type Init = (RequestInit & { signal?: AbortSignal | null }) | undefined

const NativeRequest = globalThis.Request
const nativeFetch = globalThis.fetch

function withoutSignal(init: Init): RequestInit | undefined {
  if (!init || init.signal == null) return init
  const rest = { ...init }
  delete rest.signal
  return rest
}

function abortError() {
  return new DOMException('The operation was aborted.', 'AbortError')
}

class PatchedRequest extends NativeRequest {
  constructor(input: RequestInfo | URL, init?: Init) {
    super(input, withoutSignal(init))
  }
}

globalThis.Request = PatchedRequest as typeof Request

globalThis.fetch = function patchedFetch(input: RequestInfo | URL, init?: Init) {
  const signal = init?.signal
  if (!signal) return nativeFetch(input, init)
  if (signal.aborted) return Promise.reject(abortError())

  return Promise.race([
    nativeFetch(input, withoutSignal(init)),
    new Promise<Response>((_resolve, reject) => {
      signal.addEventListener('abort', () => reject(abortError()), { once: true })
    }),
  ])
} as typeof fetch
