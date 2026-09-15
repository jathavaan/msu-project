// Must come first: it rewrites fetch/Request before MSW wraps them.
import './abortSignalShim'
import '@testing-library/jest-dom/vitest'
import { afterAll, afterEach, beforeAll } from 'vitest'
import { cleanup } from '@testing-library/react'
import { server } from './server'

// Any request a test did not explicitly stub is an error rather than a silent pass-through — an
// unhandled call means the component is hitting an endpoint the test does not know about.
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))

afterEach(() => {
  server.resetHandlers()
  cleanup()
})

afterAll(() => server.close())
