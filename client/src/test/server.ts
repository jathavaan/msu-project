import { setupServer } from 'msw/node'

/**
 * One MSW server for the whole run, started in setup.ts with no default handlers — each test
 * declares the endpoints it expects via `server.use(...)`. Intercepting at the network layer
 * (rather than mocking the generated RTK Query hooks) means tests exercise the real apiBaseQuery:
 * the Response<T> envelope unwrapping and the ProblemDetails -> ApiError normalisation both run.
 */
export const server = setupServer()
