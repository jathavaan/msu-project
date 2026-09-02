# FinanceOne — Repo Guide

Personal finance helper: track income/expenses, budgets, saving goals, upcoming payments, and a
balance forecast (see `README.md` for the full feature scope).

## Layout

```
client/    React 19 + TypeScript + Vite frontend
server/
  FinanceOne/          .NET 10 solution
    FinanceOne.Api/     Minimal API backend — Vertical Slice Architecture
    FinanceOne.Test/    xUnit integration tests, mirrors FinanceOne.Api/Features/
k8s/       Kubernetes manifests (client/server deployment + service)
docker-compose.yml   Local dev stack: SQL Server db + server + client
```

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

`docker-compose.yml` at the repo root runs the full stack (SQL Server, API on `:8080`, client on
`:5173`) — needs an `.env` with `MSSQL_SA_PASSWORD` set. The API also seeds dev-only fake data on
startup (see `server/FinanceOne/CLAUDE.md` → Persistence/Seed).
