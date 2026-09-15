# FinanceOne Client — Frontend Guide

React 19 + TypeScript + Vite, styled with Tailwind CSS, state managed with Redux Toolkit / RTK
Query. Organized as **feature folders** that mirror the backend's vertical slices — each feature
under `src/features/<name>/` owns its own API calls, types, and components.

## Stack

- **Routing:** `react-router-dom` (`src/app/router.tsx`)
- **Server state:** `@reduxjs/toolkit` + RTK Query. One base `apiSlice` (`src/app/apiSlice.ts`,
  no endpoints of its own) that every feature injects its own endpoints into via
  `apiSlice.injectEndpoints` — each feature's `api.ts` is effectively its own "slice" of the API
  surface, the RTK equivalent of the backend's per-feature endpoint groups.
- **Styling:** Tailwind CSS v4 (`@tailwindcss/vite` plugin, no separate config file — theme tokens
  live in `src/index.css`'s `@theme` block).
- **Icons:** `lucide-react`. **Charts:** `recharts` (dashboard only).

## Folder structure

```
src/
  app/            store.ts, apiSlice.ts, hooks.ts (typed useAppDispatch/useAppSelector), router.tsx
  layouts/        AppLayout.tsx (sidebar + <Outlet/>), Sidebar.tsx
  components/     Generic, feature-agnostic UI atoms (Button, Card, Modal, Input, Select,
                  ProgressBar, EmptyState, ErrorBanner, Spinner, PageHeader, QueryState)
  lib/            apiBaseQuery.ts, formatters.ts, categoryColor.ts, types.ts (shared wire types)
  features/
    <name>/
      types.ts    Request/response shapes, named to mirror the backend Command/Query/Vm they map to
      api.ts       RTK Query endpoints for this feature (injectEndpoints into apiSlice)
      <X>Form.tsx  Create/edit form, used inside a <Modal>
      <X>Page.tsx  The routed page: list + create/edit/delete wiring
```

A feature's files only import from `lib/`, `components/`, and its own folder — except where a
form legitimately needs another feature's data (e.g. Income/Expenses forms use
`features/categories/CategoryPicker`), which is expected and fine, unlike the backend's stricter
slice-isolation rule.

## The `Response<T>` envelope

Every backend query wraps its result in `{ result, errorCode, errorMessage }`
(`Common/Response.cs`); mutations mostly return a plain value (`Guid` on create) or no body at all
(`204` on update/delete). `src/lib/apiBaseQuery.ts` handles this once, centrally: it unwraps the
envelope for every endpoint automatically, and turns both `ProblemDetails` and
`ValidationProblemDetails` error bodies into one `{ status, message }` `ApiError` shape. Feature
`api.ts` files never need to know about the envelope — they just declare the unwrapped
request/response types.

RTK Query's generated `error` state is typed `ApiError | SerializedError | undefined` (the latter
covers errors thrown outside the base query). Always read it through
`getErrorMessage(error)` (`src/lib/apiBaseQuery.ts`) rather than `error.message` directly.

## ⚠️ `CategoryType` is numeric on the wire, not a string

`Domain/Enums/CategoryType.cs` is a normal C# enum (`Income = 0, Expense = 1`), and the backend
registers no `JsonStringEnumConverter`. System.Text.Json's default enum handling serializes enums
as their **numeric value**, so every request and response carries `0` (Income) or `1` (Expense) —
never the strings `"Income"`/`"Expense"`. `src/lib/types.ts` models this as a const object, not a
TS `enum` (`erasableSyntaxOnly` in `tsconfig.app.json` forbids real enums):

```ts
export const CategoryType = { Income: 0, Expense: 1 } as const
export type CategoryType = (typeof CategoryType)[keyof typeof CategoryType]
```

Use `CategoryType.Income` / `CategoryType.Expense` everywhere — never a string literal. HTML
`<select>` values are always strings on the DOM regardless, so `CategoryForm.tsx` converts at the
form boundary (`String(CategoryType.Income)` for the option value, `Number(type)` on submit) —
that's the pattern to follow for any other numeric field driven by a `<select>`.

## Environment / API base URL

Vite bakes `import.meta.env.VITE_API_BASE_URL` in at build time, so each deploy target needs its
own value:

| Target | Source | Value |
|---|---|---|
| `npm run dev` | `.env.development` | `http://localhost:5205` (the `dotnet run` http profile) |
| `docker-compose up` | `docker-compose.yml` client build `args` | `http://localhost:8080` |
| AKS (CI build) | `Dockerfile` `ARG VITE_API_BASE_URL` default | the AKS server LoadBalancer IP |

A Docker build ARG/ENV takes priority over `.env.production` (real env vars beat `.env` files in
Vite's `loadEnv`), so `docker-compose.yml`'s `build.args` is what actually controls the
docker-compose value even though `.env.production` also exists as a plain-`npm run build`
fallback.

## Local dev

`npm run dev` (needs the API running separately — see the root `CLAUDE.md`), `npm run build`
(`tsc -b && vite build`), `npm run lint`.

## Testing

**Vitest + React Testing Library + MSW**, configured in the `test` block of `vite.config.ts`
(jsdom environment, globals on). `npm test` runs once, `npm run test:watch` watches,
`npm run test:coverage` adds a v8 coverage report.

Tests live next to what they test as `<Name>.test.tsx`, inside `src/`, so `tsc -b` type-checks them
as part of `npm run build` — a type error in a test fails the build, not just the test run.

`src/test/` holds the shared harness:

| File | Purpose |
|---|---|
| `setup.ts` | Registers jest-dom matchers, starts/stops the MSW server, cleans up after each test. `onUnhandledRequest: 'error'` — a request no test stubbed fails that test rather than hanging. |
| `server.ts` | The MSW server. No default handlers; each test declares its own with `server.use(...)`. |
| `renderWithStore.tsx` | Renders inside a **fresh** Redux store + `MemoryRouter`. Fresh matters: RTK Query's cache lives in the store, so a shared one would let one test's data satisfy the next test's query. Also exports `API_URL` for building MSW handlers. |
| `abortSignalShim.ts` | See below. |

Mocking at the network layer (MSW) rather than mocking the generated RTK Query hooks is deliberate:
it means `apiBaseQuery` actually runs, so the `Response<T>` unwrapping and the
`ProblemDetails`/`ValidationProblemDetails` → `ApiError` normalisation are covered by the component
tests instead of needing tests of their own.

### ⚠️ `abortSignalShim.ts`

jsdom supplies its own `AbortController`/`AbortSignal` but no `fetch`/`Request` — those stay Node's
(undici), which brand-checks `RequestInit.signal` against *its* `AbortSignal` and throws
`Expected signal to be an instance of AbortSignal` on jsdom's. RTK Query attaches one to every
request, so without the shim every test that touches the API fails. The shim drops the signal at
the `fetch`/`Request` boundary and re-implements the one observable effect (the promise rejecting
on abort). Remove it only after verifying the underlying jsdom/undici mismatch is gone.

### Scope

Covered today: `lib/` formatters and colours, the shared `components/` atoms, `BudgetCard`,
`CategoryForm` (including the numeric-`CategoryType` form boundary), and `BudgetsPage` (envelope
unwrapping, error banners, the delete confirmation). Other features follow the same shape.

### Writing tests for new code

**New component or helper means a new `<Name>.test.tsx` next to it.** What to cover depends on
what the thing is:

| Adding a… | Cover |
|---|---|
| `lib/` helper | Its edge cases directly — no rendering. Boundaries (`formatRecurrenceDay`'s 11/12/13), and anything derived from `Date` under `vi.useFakeTimers()` + `vi.setSystemTime()`. |
| `components/` atom | That it renders its props, that callbacks fire on interaction, and each conditional branch (`error`, `placeholder`, `disabled`, the tone thresholds on `ProgressBar`). |
| `<X>Form.tsx` | Prefill when editing vs. create defaults, **the exact request body it sends**, and that an API error is surfaced while `onDone` is *not* called. |
| `<X>Page.tsx` | Loaded list, empty state, error banner, and that a destructive action goes through its `ConfirmDialog`. |

Conventions that keep these honest:

- **Query by role or label, not by test id.** `getByLabelText('Type')` works because `Input`/`Select`
  wire a generated id to their label — a test that needs a test id usually means the markup is not
  accessible.
- **Anything using an RTK Query hook renders through `renderWithStore`**, never bare `render` —
  it supplies the Redux and router context, with a fresh store per test.
- **Stub the network, not the hooks.** Declare handlers with `server.use(http.get(\`${API_URL}/…\`))`
  rather than `vi.mock`-ing a feature's `api.ts`. That keeps `apiBaseQuery` in the loop, so the
  `Response<T>` unwrapping and error normalisation stay covered.
- **Wrap a query response in the envelope** (`{ result, errorCode: null, errorMessage: null }`) —
  that is what the backend actually returns. Mutations return a bare value or `204`.
- **An unstubbed request fails the test** (`onUnhandledRequest: 'error'`). That is deliberate: it is
  how a test proves a *cancelled* delete sent no `DELETE`. Stub every endpoint the component
  touches, including ones a child form loads.
- **Read a controlled input's value inside the handler** (`onChange={e => spy(e.target.value)}`),
  not from `mock.calls[0][0].target.value` afterwards — React re-renders the node back to its prop.
- **Assert `formatCurrency` output loosely.** It is `nb-NO`/NOK, so it contains non-breaking spaces
  and a U+2212 minus sign, not an ASCII hyphen. Match on a fragment or a regex.
- Never edit `src/test/abortSignalShim.ts` casually — see the warning above.
