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
(`tsc -b && vite build`), `npm run lint`. No test runner configured yet.
