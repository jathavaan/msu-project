# FinanceOne — Repo Guide

Personal finance helper: track income/expenses, budgets, saving goals, upcoming payments, and a
balance forecast (see `README.md` for the full feature scope).

## Layout

```
client/    React 19 + TypeScript + Vite frontend
server/
  FinanceOne/          .NET 10 solution
    FinanceOne.Api/              Minimal API backend — Vertical Slice Architecture
    FinanceOne.UnitTests/        xUnit unit tests, mirrors FinanceOne.Api/Features/
    FinanceOne.IntegrationTests/ xUnit tests against a real MySQL (Testcontainers), same mirror
k8s/       Kubernetes manifests (client/server deployment + service)
docker-compose.yml   Local dev stack: MySQL db + server + client
```

## CI

`.github/workflows/build-and-test.yaml` is a reusable workflow holding the whole check suite —
two build jobs that fan out into three parallel test jobs (server unit, server integration,
client). Both `pull-request.yaml` (on every PR) and `build-and-deploy.yaml` (before pushing images
to ACR) call it, so merge gates and deploy gates are the same checks by construction. Add a new
kind of check there, not to either caller.

Each test job publishes its results through `dorny/test-reporter`, so a failing test shows up as a
named check run with the assertion message annotated onto the diff rather than only as a red X.
That needs `checks: write`, which each caller grants **on the job that calls the workflow** — never
at the top of the file, so the deploy jobs holding the Azure OIDC token keep the narrower grant
they have today. The server jobs feed it the `.trx` that `dotnet test --logger` already writes; the
client job needs JUnit XML, which `vite.config.ts` emits only when `CI` is set.

### Where the results show up

Two different places, which are easy to confuse:

| What | Where | Notes |
|---|---|---|
| **Check runs** (test-reporter) | PR → **Checks** tab → sidebar: `Server unit tests`, `Server integration tests`, `Client tests` | Pass/fail counts and per-test detail. These are separate entries from the jobs themselves. Failures are also annotated inline on the **Files changed** tab. |
| **Raw result files** | Workflow run summary page → **Artifacts** at the bottom | `server-unit-test-results` / `server-integration-test-results` (`.trx`), `client-test-results` (`junit.xml`). Zipped, 90-day default retention. |

Both are per-run, so an old run keeps its own results. For a `workflow_dispatch` run there is no PR,
so the check runs hang off the commit instead — reach them from the commit's status icon.

Because the reporter only *reads* files, it never makes a job fail on its own
(`fail-on-error: false`): the `dotnet test` / `npm test` step already did that. If a check run is
missing, the usual cause is the test step dying before writing its file at all.

## CD

`build-and-deploy.yaml` runs on push to `main`. Server and client are independent chains below the
shared gate:

```
changes ─┬─────────────────────────────────────────────┐
         │                                             │
test ────┼→ build-and-push-server → deploy-server      │  (migrations, then rollout)
         └→ build-and-push-client → deploy-client      │
```

`test` calls the same `build-and-test.yaml` the PR workflow does and has **no** path filter, so
both the frontend and the backend suites must pass before *either* chain pushes an image — a
client-only change still has to keep the backend green. `changes` (path filters) only decides
which chain runs at all; a `workflow_dispatch` run forces both.

Each `deploy-*` job ends with `kubectl rollout status --timeout=300s`. That is a real gate now that
both deployments have readiness probes: a pod that never becomes Ready fails the job, and because
a rollout replaces pods only as new ones become Ready, the previous version keeps serving. On
failure the job dumps `kubectl describe` and recent pod logs.

## Tests come with the code

New code ships with its tests in the same change — a backend slice without handler/validator/
integration tests, or a component without a `.test.tsx`, is not finished. Both guides have a
"writing tests" section naming exactly which files to add and what to cover; read it before
writing the production code, not after. `npm test` and `dotnet test` both run in seconds locally.

## Backend work

**Before touching anything under `server/`, read `server/FinanceOne/CLAUDE.md`.** It defines the
VSA slice structure, naming conventions, repository/DI/validation patterns, and testing approach
this codebase follows — those conventions aren't optional style, DI registration and endpoint
discovery in `FinanceOne.Api` actually rely on them being followed consistently.

## Frontend work

**Before touching anything under `client/`, read `client/CLAUDE.md`.** It covers the feature-folder
structure, Redux Toolkit / RTK Query conventions, and a non-obvious wire-format gotcha
(`CategoryType` serializes as a number, not a string) that isn't discoverable from the code alone.

## Local dev

`docker-compose.yml` at the repo root runs the full stack (MySQL, API on `:8080`, client on
`:5173`) — needs an `.env` with `MYSQL_PASSWORD` set. The API also seeds dev-only fake data on
startup (see `server/FinanceOne/CLAUDE.md` → Persistence/Seed).
