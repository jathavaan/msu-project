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
infra/     Bicep IaC for the Azure resources in rg-financeone-msu (AKS, ACR, MySQL, Key Vault, etc.)
docker-compose.yml   Local dev stack: MySQL db + server + client
```

## CI

`.github/workflows/build-and-test.yaml` is a reusable workflow holding the whole check suite —
two build jobs that fan out into three parallel test jobs (server unit, server integration,
client). Both `pull-request.yaml` (on every PR) and `build-and-deploy.yaml` (before pushing images
to ACR) call it, so merge gates and deploy gates are the same checks by construction. Add a new
kind of check there, not to either caller.

It takes two boolean inputs, `run-server` and `run-client`, both defaulting to `true`. Only
`pull-request.yaml` passes them — computed from a `changes` job (`dorny/paths-filter` on
`server/**` / `client/**`, plus a catch-all `other` filter for anything outside both that forces
both suites, same as `workflow_dispatch`) — so a PR touching only one side skips the other side's
build+test jobs for faster feedback. `build-and-deploy.yaml` never passes them, so its call keeps
getting the full suite unconditionally on every push to `main` — a client-only push still has to
keep the backend green before shipping.

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
shared gate, and both wait on `infra-deploy` before actually rolling out:

```
          ┌→ infra-deploy ───────────────────────────────────┐
changes ──┤                                                  │
          └→ test ─┬→ build-and-push-server → deploy-server ─┤
                    └→ build-and-push-client → deploy-client ─┘
```

`test` calls the same `build-and-test.yaml` the PR workflow does and has **no** path filter, so
both the frontend and the backend suites must pass before *either* chain pushes an image — a
client-only change still has to keep the backend green. It `needs: changes` purely so the run
graph reads detect-then-act; `changes`'s outputs don't gate whether `test` runs, only what runs
after it — `infra-deploy`, `build-and-push-server`, and `build-and-push-client` are the jobs
actually skipped per-service when their path filter doesn't match. A `workflow_dispatch` run
forces all three of those regardless of what changed.

`infra-deploy` runs the real Bicep apply (see Infrastructure below) ahead of the AKS deploy jobs, so
app code that depends on new infra never rolls out before that infra exists. `deploy-server` and
`deploy-client` only require `infra-deploy` to be `success` **or** `skipped` — a push that doesn't
touch `infra/**` deploys exactly as fast as it did before this job existed; one that does waits for
the `infra-production` approval and apply to finish first.

Each `deploy-*` job ends with `kubectl rollout status --timeout=300s`. That is a real gate now that
both deployments have readiness probes: a pod that never becomes Ready fails the job, and because
a rollout replaces pods only as new ones become Ready, the previous version keeps serving. On
failure the job dumps `kubectl describe` and recent pod logs.

## Infrastructure

`infra/` is Bicep IaC for every Azure resource in the single `rg-financeone-msu` resource group —
`main.bicep` composes one module per resource (or tight group of resources) under `infra/modules/`,
parameterized by the single `main.parameters.json` (one environment, no dev/staging). Resources
that predate this template (AKS, ACR, MySQL, Key Vault, the `financeone-uami` identity) are adopted
by matching their live config, not recreated — a first deployment against the real resource group
should be close to a no-op. New modules (Log Analytics, App Insights, Monitor alerts, the app
Storage account) provision resources that didn't exist before this template.

`.github/workflows/infra.yaml` is a reusable workflow (`on: workflow_call`, same shape as
`build-and-test.yaml`) that runs `az deployment group what-if` and posts the result as a PR
comment. `pull-request.yaml` calls it, gated on a `changes` job so it only runs when a PR touches
`infra/**` (or on `workflow_dispatch`) — that keeps it inside `pull-request.yaml`'s
cancel-in-progress concurrency group instead of piling up its own parallel runs on repeated pushes.
The real `az deployment group create` runs as the
`infra-deploy` job in `build-and-deploy.yaml` instead (see CD above) — that's what lets it be
sequenced ahead of the AKS deploy jobs in the same run. It's still gated behind the
`infra-production` GitHub Environment, so an apply always needs a manual approval click even though
the workflow itself is unattended. `financeone-uami` holds `Contributor` on `rg-financeone-msu` for
this — enough for every resource type in `main.bicep`.

`infra-deploy` never passes `--mode`, so it deploys **Incremental** (the CLI default): it only
creates/updates what's declared in `main.bicep` and leaves everything else in the resource group
alone, even a resource that used to be in Bicep and got deleted from the template. `infra.yaml`
runs a second, Complete-mode `what-if` to surface that gap — Complete mode only changes what
`what-if` *reports* (`what-if` never applies anything, in either mode), so a resource that's in the
resource group but not in `main.bicep` shows up as a "Delete" change in that preview and gets called
out in the PR comment, without `infra-deploy` itself ever actually deleting anything.
`Microsoft.Authorization/roleAssignments` is excluded from that report, since
`infra/role-assignments.bicep` is deliberately kept out of `main.bicep` (see below) and would
otherwise show up as permanent false-positive drift.

Actually deleting that drift is a separate, deliberately opt-in path: `infra.yaml`'s `prune-drift`
job, triggered only via `workflow_dispatch` with `action: prune-drift` — never by a PR or a plain
merge to `main`. It requires the `confirm` input to exactly match `rg-financeone-msu` (checked by
the `validate-prune-request` job before anything reaches the `infra-production` approval gate, so a
mistaken trigger fails without spending an approver's review click) and still goes through that same
`infra-production` environment approval. Check the `what-if` job's drift report *before* triggering
it — the environment gate fires before any step in `prune-drift` runs, so the approver has no live
diff to review at approval time. The job runs `az deployment group create --mode Complete` for real,
which deletes any resource in `rg-financeone-msu` not declared in `main.bicep`. Per Azure's own
deployment-mode semantics, Complete mode's deletion only reaches standard resource-group-level
resources — it doesn't touch extension resources like role assignments, locks, or policy
assignments, so `infra/role-assignments.bicep` is unaffected without needing any extra filtering
there either. `infra-deploy`'s regular apply is untouched by any of this and stays on Incremental.

`infra/role-assignments.bicep` is a second, separate template holding every
`Microsoft.Authorization/roleAssignments` this project needs (the grants financeone-uami and the AKS
kubelet identity hold on ACR/AKS/Key Vault/Storage). It is deliberately **not** wired into
`main.bicep` or the `infra-deploy` job: this subscription has an ABAC condition that allows delegating
`Contributor` but blocks delegating `User Access Administrator`, so financeone-uami can never itself
hold `roleAssignments/write` and CI can never run it. Apply it by hand, with an account that has
that write permission, whenever a role changes:
```
az deployment group create -g rg-financeone-msu -f infra/role-assignments.bicep -p infra/role-assignments.parameters.json
```
The `infra-production` environment needs a required reviewer configured (Settings → Environments) —
also one-time manual setup, not something a Bicep deployment can grant itself.

The Entra ID app registration for multi-user auth (a separate, later issue) is a Microsoft Graph
object, not a native ARM/Bicep resource, and stays a documented manual `az ad app create` step
rather than fighting the Microsoft.Graph Bicep extension.

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

## Issues and PRs

Use the repo's templates rather than freeform text: issues follow `.github/ISSUE_TEMPLATE/`
(`bug_report.yml` or `feature_request.yml`), and PRs follow `.github/PULL_REQUEST_TEMPLATE.md`
(link the issue with `Closes #`, fill in the type and testing checklists).

## Local dev

`docker-compose.yml` at the repo root runs the full stack (MySQL, API on `:8080`, client on
`:5173`) — needs an `.env` with `MYSQL_PASSWORD` set. The API also seeds dev-only fake data on
startup (see `server/FinanceOne/CLAUDE.md` → Persistence/Seed).
