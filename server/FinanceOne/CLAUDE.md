# FinanceOne.Api — Backend Guide

.NET 10 minimal API backend using **Vertical Slice Architecture (VSA)**. Every feature is a
self-contained slice under `Features/`, not a layer spread across shared controllers/services/
repositories. Single Responsibility Principle governs slice internals: each class does exactly
one job, and a slice only contains the files it actually needs.

## Project layout

```
FinanceOne.Api/
  Domain/Entites/        Code-first EF entities (cross-cutting, shared by every slice)
  Domain/Enums/
  Configurations/         One IEntityTypeConfiguration<T> per entity (EF relations/constraints)
  Persistence/            FinanceOneDbContext, migrations, dev seeder
  Common/                 Cross-cutting plumbing shared by all slices (see below)
  Features/
    <Group>/              e.g. Budgets, Categories, Expenses
      <Group>Endpoints.cs      Route group + aggregates each slice's Map<Slice>()
      <Slice>/             e.g. CreateBudget, GetBudgets, UpdateBudget
        <Slice>Command.cs  or <Slice>Query.cs
        <Slice>Handler.cs
        I<Slice>Repository.cs / <Slice>Repository.cs
        <Slice>Validator.cs      (only if the request has input to validate)
        <Slice>Endpoint.cs
        <Name>Vm.cs              (only for queries that shouldn't return raw entities)
        README.md                 (already exists — describes endpoint behavior/business rules)
FinanceOne.UnitTests/
  Common/                                              Cross-cutting tests (Response, DI conventions)
  Features/<Group>/<Slice>/<Slice>HandlerTests.cs      Mirrors the Features/ tree 1:1
  Features/<Group>/<Slice>/<Slice>ValidatorTests.cs
FinanceOne.IntegrationTests/
  Common/                                              MySqlFixture, IntegrationTest base class
  Features/<Group>/<Slice>/<Slice>Tests.cs             Mirrors the Features/ tree 1:1
```

## Anatomy of a slice

A slice is the unit of change. Everything a feature needs to go from HTTP request to response
lives in its own folder, named after the use case (`CreateBudget`, `GetBudgetById`, not
`Budget`). Namespace matches the folder path:
`FinanceOne.Api.Features.Budgets.CreateBudget`.

Only include the files a slice actually needs — SRP applies to the *set* of files too. A query
with no filters needs no validator; a slice with no read/write to the DB (rare) needs no
repository.

| File | Purpose |
|---|---|
| `<Slice>Command.cs` / `<Slice>Query.cs` | The request DTO. Implements `IRequest<Response<T>>` (see `Common/Requests.cs`). Commands mutate, queries read — name accordingly. |
| `<Slice>Handler.cs` | Implements `IRequestHandler<TRequest, Response<T>>`. Orchestrates: calls the validator's result (already enforced upstream by the endpoint filter — see Validation), calls the repository, applies business rules, maps to a Vm if needed, returns `Response<T>`. No EF/SQL code here — that belongs in the repository. |
| `I<Slice>Repository.cs` + `<Slice>Repository.cs` | Interface + implementation. Injects `FinanceOneDbContext` directly. Contains exactly the queries/persistence this one slice needs. |
| `<Slice>Validator.cs` | `AbstractValidator<TRequest>` (FluentValidation). Only for slices with request input worth validating. |
| `<Slice>Endpoint.cs` | Static class with a `Map<Slice>(this RouteGroupBuilder group)` extension that maps the single HTTP route and wires the request through `Send`-style dispatch to the handler. |
| `<Name>Vm.cs` | View model — see View models below. |

### Example: `Features/Budgets/CreateBudget/`

```csharp
// CreateBudgetCommand.cs
namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public sealed record CreateBudgetCommand(Guid CategoryId, decimal MonthlyLimit)
    : IRequest<Response<Guid>>;
```

```csharp
// CreateBudgetValidator.cs
namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.MonthlyLimit).GreaterThan(0);
    }
}
```

```csharp
// ICreateBudgetRepository.cs / CreateBudgetRepository.cs
namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public interface ICreateBudgetRepository
{
    Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> BudgetExistsForCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<Guid> Add(Budget budget, CancellationToken cancellationToken);
}

public sealed class CreateBudgetRepository(FinanceOneDbContext context) : ICreateBudgetRepository
{
    public Task<Category?> GetExpenseCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public Task<bool> BudgetExistsForCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Budgets.AnyAsync(b => b.CategoryId == categoryId, cancellationToken);

    public async Task<Guid> Add(Budget budget, CancellationToken cancellationToken)
    {
        context.Budgets.Add(budget);
        await context.SaveChangesAsync(cancellationToken);
        return budget.Id;
    }
}
```

```csharp
// CreateBudgetHandler.cs
namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public sealed class CreateBudgetHandler(ICreateBudgetRepository repository)
    : IRequestHandler<CreateBudgetCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetExpenseCategory(request.CategoryId, cancellationToken);
        if (category is null || category.Type != CategoryType.Expense)
        {
            return Response<Guid>.Failure(StatusCodes.Status404NotFound, "Expense category not found.");
        }

        if (await repository.BudgetExistsForCategory(request.CategoryId, cancellationToken))
        {
            return Response<Guid>.Failure(StatusCodes.Status409Conflict, "A budget already exists for this category.");
        }

        var budget = new Budget { Id = Guid.NewGuid(), CategoryId = request.CategoryId, MonthlyLimit = request.MonthlyLimit };
        var id = await repository.Add(budget, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
```

```csharp
// CreateBudgetEndpoint.cs
namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public static class CreateBudgetEndpoint
{
    public static RouteGroupBuilder MapCreateBudget(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateBudgetCommand command, CreateBudgetHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/budgets/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateBudgetCommand>>();

        return group;
    }
}
```

### Example query: `Features/Budgets/GetBudgets/` (uses a Vm)

`GetBudgets` needs "used this month", which requires reading `Expenses` — a table `Budgets`
doesn't own. That's fine: the slice's own repository queries whatever tables it needs.

```csharp
// BudgetVm.cs
namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public sealed record BudgetVm(Guid Id, Guid CategoryId, string CategoryName, decimal MonthlyLimit, decimal UsedThisMonth);
```

```csharp
// IGetBudgetsRepository.cs / GetBudgetsRepository.cs — queries Budgets + Categories + Expenses directly
public interface IGetBudgetsRepository
{
    Task<List<BudgetVm>> GetBudgetsWithUsage(CancellationToken cancellationToken);
}
```

The handler calls the repository and returns `Response<List<BudgetVm>>.Success(...)` — mapping
happens in the repository/handler for this slice, inline, with no shared mapper class.

## Naming conventions

| Concept | Pattern | Example |
|---|---|---|
| Command (mutation) | `<Slice>Command.cs` | `CreateBudgetCommand`, `UpdateBudgetCommand`, `DeleteBudgetCommand` |
| Query (read) | `<Slice>Query.cs` | `GetBudgetsQuery`, `GetBudgetByIdQuery` |
| Handler | `<Slice>Handler.cs` | `CreateBudgetHandler` |
| Repository | `I<Slice>Repository` / `<Slice>Repository` | `ICreateBudgetRepository` / `CreateBudgetRepository` |
| Validator | `<Slice>Validator.cs` | `CreateBudgetValidator` |
| Endpoint | `<Slice>Endpoint.cs`, method `Map<Slice>` | `CreateBudgetEndpoint.MapCreateBudget` |
| View model | `<Name>Vm.cs` | `BudgetVm` |
| Feature group aggregator | `<Group>Endpoints.cs`, method `Map<Group>Endpoints` | `BudgetsEndpoints.MapBudgetsEndpoints` |
| Test class | `<Slice>Tests.cs` | `CreateBudgetTests` |

## Requests, responses, `Unit`

Already defined in `Common/`:

- `IRequest<TResponse>` / `IRequestHandler<TRequest, TResponse>` — the command/query contract (no MediatR; this is our own minimal dispatch).
- `Response<TResult>` — every handler returns this. `Response<T>.Success(result)` / `Response<T>.Failure(errorCode, errorMessage)`. `ErrorCode` is an HTTP status code.
- `Unit` — stand-in payload for commands with no meaningful return value (e.g. `DeleteBudgetHandler` returns `Response<Unit>`).

## Repositories

- **One repository per slice.** `I<Slice>Repository` lives inside the slice folder, not shared across slices.
- A slice's repository queries **whatever tables it needs** via `FinanceOneDbContext`, even entities owned by other feature groups (e.g. `GetBudgetsRepository` reads `Expenses`). Don't inject one slice's repository into another slice — duplication of a query shape across slices is expected and fine in VSA; a shared repository/service is only worth introducing if the exact same nontrivial computation is needed in 3+ places, and even then, discuss it before adding a shared abstraction.
- Repositories are the *only* place EF Core/LINQ queries live. Handlers never touch `FinanceOneDbContext` directly.

## Validation (FluentValidation)

- One validator per slice (`<Slice>Validator : AbstractValidator<TRequest>`), only when the request has input worth validating.
- Wired through a single shared generic endpoint filter, not called manually in each handler:

```csharp
// Common/ValidationFilter.cs
namespace FinanceOne.Api.Common;

public sealed class ValidationFilter<TRequest> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().First();
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
            {
                return Results.ValidationProblem(result.ToDictionary());
            }
        }

        return await next(context);
    }
}
```

Each endpoint that needs validation adds `.AddEndpointFilter<ValidationFilter<TRequest>>()` when
mapping its route (see `CreateBudgetEndpoint` above). If a slice has no validator registered in
DI, the filter is a no-op — safe to add to every mutating endpoint by default.

Add the `FluentValidation` and `FluentValidation.DependencyInjectionExtensions` packages to
`FinanceOne.Api.csproj` (not yet referenced).

## Endpoints (minimal APIs)

- Each slice maps its **own single route** via a `Map<Slice>(this RouteGroupBuilder group)` extension in its `<Slice>Endpoint.cs`.
- Each feature group has one `<Group>Endpoints.cs` that opens the route group and calls every slice's `Map<Slice>()`:

```csharp
// Features/Budgets/BudgetsEndpoints.cs
namespace FinanceOne.Api.Features.Budgets;

public static class BudgetsEndpoints
{
    public static void MapBudgetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapCreateBudget();
        group.MapGetBudgets();
        group.MapGetBudgetById();
        group.MapUpdateBudget();
        group.MapDeleteBudget();
    }
}
```

- `Program.cs` calls one line per feature group:

```csharp
app.MapBudgetsEndpoints();
app.MapCategoriesEndpoints();
app.MapExpensesEndpoints();
// ...
```

Adding a new feature group means adding one line to `Program.cs`; adding a new slice to an
existing group means adding one line to that group's `<Group>Endpoints.cs`. Routes for a slice
are never mapped anywhere else.

## Dependency injection

No manual per-class registration. A single startup extension scans the assembly and registers
by **interface implementation** — no marker interfaces:

```csharp
// Common/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceOneServices(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;

        // IRequestHandler<,> implementations
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
        {
            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
                services.AddScoped(type); // handler also resolvable directly for the endpoint
            }
        }

        // *Repository implementations (interface I<Name>Repository -> class <Name>Repository)
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && t.Name.EndsWith("Repository")))
        {
            var repoInterface = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}");
            if (repoInterface is not null)
            {
                services.AddScoped(repoInterface, type);
            }
        }

        // FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
```

`Program.cs` calls `builder.Services.AddFinanceOneServices();` once. **Adding a new slice never
requires touching DI registration** — it's picked up automatically as long as the naming
conventions above are followed (this is why the naming conventions matter, not just for
readability).

## Error handling

- **Expected failures** (not found, conflict, invalid state) are returned, never thrown: `Response<T>.Failure(statusCode, message)`. The endpoint translates this into the right HTTP response (see `CreateBudgetEndpoint` above).
- **Unexpected failures** (DB unavailable, bugs, anything not part of the business logic being modeled) are allowed to throw and are caught centrally:

```csharp
// Common/GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { Status = 500, Title = "An unexpected error occurred." }, cancellationToken);
        return true;
    }
}
```

Registered once: `builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); builder.Services.AddProblemDetails();` and `app.UseExceptionHandler();`.

Never use exceptions for expected business-rule branching (e.g. don't throw `NotFoundException`
for a missing budget — return `Response<T>.Failure`).

## View models

Not every slice needs one. Use a `<Name>Vm.cs` **only when** the shape returned to the client
should differ from the raw entity — e.g. it aggregates data across entities (`BudgetVm` above),
needs to omit EF navigation properties, or reshapes data for the frontend. A command that only
returns a generated id (`Response<Guid>`) or `Response<Unit>` needs no Vm.

- Lives inside the slice folder that produces it.
- Mapping from entity → Vm happens inline in the handler (or repository, for slices where the
  repository already shapes the query result) — no shared/central mapper class.
- Named after what it represents (`BudgetVm`, not `GetBudgetsVm`), so it can be reused by
  another slice's *own* Vm-producing code if genuinely identical — though duplicating a small Vm
  per slice is also fine and often clearer.

## Domain & persistence (cross-cutting, outside Features/)

- `Domain/Entites/` — code-first EF entities shared by every slice that touches that table. (Folder is spelled `Entites` — keep matching that existing spelling rather than "fixing" it in isolation.)
- `Domain/Enums/`
- `Configurations/` — one `IEntityTypeConfiguration<T>` per entity; relations/constraints/precision live here, not in slices. Applied via `ApplyConfigurationsFromAssembly` in `FinanceOneDbContext`.
- `Persistence/FinanceOneDbContext.cs` — one `DbSet<T>` per entity; add here when a new entity is introduced.
- `Persistence/Migrations/` — run `dotnet ef migrations add <Name>` from `FinanceOne.Api/` after changing an entity or its configuration. Never applied by the running app itself — `dotnet FinanceOne.Api.dll --migrate` (handled early in `Program.cs`, before Kestrel starts) applies pending migrations and exits, using the same image as the app so migrations always match the code they ship with. In AKS this runs as a one-off `k8s/server-migration-job.yaml` Job, applied by the CI/CD workflow before the deployment is rolled. In docker-compose local dev, the `migrate` service runs the same `--migrate` flag once against the `db` container before `server` starts (`depends_on: condition: service_completed_successfully`).
- `Persistence/Seed/FinanceOneDbSeeder.cs` — dev-only fake data, wired in `Program.cs` behind `IsDevelopment()`.

## Configuration & secrets

- `Development` (docker-compose, local `dotnet run`): `ConnectionStrings:MySql` is a full
  username/password connection string from an env var (docker-compose) or user-secrets
  (`UserSecretsId` in the csproj) — no Key Vault call, no Azure AD.
- Everywhere else (deployed to AKS, or run locally against a non-Development environment):
  `financeone-sqlserver` (the Azure Database for MySQL server) has `aad_auth_only=ON` — MySQL
  username/password auth is disabled server-wide, Azure AD tokens are the only way in. So
  `ConnectionStrings:MySql` in `appsettings.json` is a *passwordless* template (host/port/db/
  `Uid=financeone-uami`, no `Pwd`); `Program.cs` appends a short-lived Azure AD access token as
  the password at `AddDbContext` time, fetched via `DefaultAzureCredential` for the
  `https://ossrdbms-aad.database.windows.net/.default` scope — the same credential/Workload
  Identity used for Key Vault below, just a different token audience. `financeone-uami` is
  mapped to a MySQL AAD user of the same name via `CREATE AADUSER` (one-time DB-side setup, not
  in source control — see the AAD administrators on the server if this ever needs recreating).
  Tokens are cached internally by `DefaultAzureCredential` and only re-issued near expiry, so the
  connection string stays stable across most requests and ADO.NET's connection-string-keyed
  pooling still works.
- `Program.cs` also adds Azure Key Vault (`financeone-key-vault`, URI in `appsettings.json` ->
  `KeyVault:Uri`) as a configuration source outside `Development`, via `AddAzureKeyVault` +
  `DefaultAzureCredential` — same Workload Identity, `Microsoft.KeyVault/*` scope instead.
  Secret names use `--` in place of `:` (e.g. a secret `Foo--Bar` becomes config key `Foo:Bar`)
  — the Azure SDK's own convention, not custom mapping code. Nothing lives in the vault right
  now (the DB connection moved to Azure AD auth instead of a stored password), but it stays
  wired up: add new secrets there, not to `appsettings.json`/`appsettings.Development.json`
  directly, and read them through `IConfiguration` the same way.

## Health checks

Two endpoints, registered in `Program.cs` and consumed by the probes in
`k8s/server-deployment.yaml`:

| Endpoint | Runs | Answers |
|---|---|---|
| `/health/live` | nothing (`Predicate = _ => false`) | "is this process still responding?" |
| `/health/ready` | checks tagged `"ready"` — currently `AddDbContextCheck<FinanceOneDbContext>` | "can this pod actually serve a request?" |

The split is the whole point, so keep it when adding checks:

- **Never put a dependency check behind `/health/live`.** Liveness failure means *restart the pod*,
  and restarting cannot fix an unreachable MySQL — it would just restart every replica at once and
  turn a recoverable blip into an outage. A new check gets the `"ready"` tag unless there is a
  specific reason it doesn't.
- **Readiness gates the rollout.** A pod that fails `/health/ready` never joins the Service, so
  `kubectl rollout status` in the deploy workflow fails and the previous version keeps serving.
  That is what makes a broken deploy visible instead of silently succeeding.

Both are registered as **terminal middleware** (`app.UseHealthChecks(...)`) rather than routed
endpoints, positioned before the logging middleware and `UseHttpsRedirection`. Two reasons, both
easy to undo by accident:

1. Probes reach the container over plain HTTP. If an HTTPS port were ever configured, a routed
   endpoint would answer them with a 307 — which kubelet counts as success, silently disabling both
   probes.
2. A probe every few seconds per pod would otherwise emit a request-summary log line each time.

## Logging

Serilog (`Serilog.AspNetCore` + `Serilog.Enrichers.Environment`), wired in `Program.cs` via
`builder.Host.UseSerilog(...)`, replaces the default `Microsoft.Extensions.Logging` console
provider entirely — `ILogger<T>` injection works exactly as normal everywhere, it's just backed
by Serilog under the hood.

- **Levels/overrides** live in the `Serilog:MinimumLevel` section of `appsettings.json` /
  `appsettings.Development.json` (not the old `Logging:LogLevel` shape). Production overrides
  `Microsoft.EntityFrameworkCore.Database.Command` to `Warning` to stop every SQL statement
  logging at Information; Development leaves it alone so SQL is visible while debugging locally.
- **Output**: Development writes plain readable text to the console. Everywhere else writes one
  JSON object per log event (`CompactJsonFormatter`) to stdout, so pod stdout in AKS stays directly
  parseable regardless of what else is collecting it.
- **Application Insights**: `UseSerilog(..., writeToProviders: true)` lets the same `ILogger`
  events also reach whatever other `ILoggerProvider`s are registered, in addition to Serilog's own
  sinks above. When `ApplicationInsights:ConnectionString` is configured (outside Development, from
  the `ApplicationInsights--ConnectionString` Key Vault secret), `Program.cs` registers the Azure
  Monitor OpenTelemetry Distro (`Azure.Monitor.OpenTelemetry.AspNetCore`), which adds that kind of
  provider plus ASP.NET Core request traces and outgoing `HttpClient` dependency spans —
  `options.SamplingRatio = 1.0f` keeps capture at 100%, matching issue #50. Skipped entirely (no
  registration at all) when the connection string isn't set, so Development doesn't try to phone
  home. MySQL calls don't show up as dependency spans — `MySql.EntityFrameworkCore`'s driver
  (Oracle's `MySql.Data`) has no OpenTelemetry `ActivitySource` of its own.
- **Per-request correlation**: a middleware in `Program.cs` pushes `HttpContext.TraceIdentifier`
  onto Serilog's `LogContext` for the duration of each request, so every log line written while
  handling it (handler logs, `GlobalExceptionHandler`'s exception log, and the request-summary
  line from `UseSerilogRequestLogging()`) carries the same `RequestId` property — grep/filter by
  that to reconstruct everything one request did. `UseSerilogRequestLogging()` itself logs one
  summary line per request (method, path, status code, elapsed ms) and is registered before
  `UseExceptionHandler()` so it still captures the resulting status code when a handler throws.
- **Logging inside a slice**: inject `ILogger<XHandler>` (or `ILogger<XRepository>`) the same way
  as any other dependency if a slice needs to log something — nothing slice-specific required.
  There's no repo-wide convention yet for what business events are worth logging at Information;
  discuss before adding logging as a matter of course to every handler.

## Testing

Two projects, both mirroring `Features/` 1:1, split so CI can run them as separate parallel jobs
and so a unit test can never quietly acquire a database dependency.

### `FinanceOne.UnitTests/` — fast, no Docker

Handlers and validators in isolation. Repositories are substituted with **NSubstitute**, so these
tests pin down *decision logic*: which failure code a handler returns for which precondition, and
that it doesn't write when it shouldn't.

- `<Slice>HandlerTests.cs` — one test per branch through `Handle`, asserting the `Response<T>` and
  (for the failure branches) that the repository was *not* called.
- `<Slice>ValidatorTests.cs` — one per rule in the validator, using FluentValidation's
  `TestValidate` / `ShouldHaveValidationErrorFor`.
- `Common/ServiceRegistrationTests.cs` guards the convention-driven DI across **every** slice, not
  just the tested ones: it asserts each handler, repository, and validator in the assembly actually
  gets registered by `AddFinanceOneServices`. A slice that breaks the naming conventions fails here
  instead of at runtime.

### `FinanceOne.IntegrationTests/` — real MySQL via Testcontainers

The real repository and the real handler against a real database engine. This is where anything
the SQL itself decides belongs: decimal(18,2) rounding, the unique index behind "one budget per
category", the `Restrict` delete rules on `Category`, `ORDER BY` results, and case-insensitive
collation behaviour.

- `Common/MySqlFixture.cs` starts one `mysql:8.0` container for the whole assembly and applies the
  production migrations to it. All tests share it through `[Collection("Database")]`, which also
  makes them run sequentially.
- `Common/IntegrationTest.cs` is the base class: it deletes every table before each test and
  exposes `GivenCategory` / `GivenBudget` / `GivenExpense` arrange helpers plus `NewContext()` — a
  second `DbContext` for assertions, so a test can't pass on EF's change tracker alone.
- One test class per slice, covering the happy path and each documented failure case from that
  slice's `README.md` (404/409/etc.).

Every implemented feature group is covered: `Budgets`, `Categories`, `Expenses`, `Income`,
`SavingGoals`, `MonthlySavings`, `DiscountCodes`, `UpcomingPayments`, `BalanceForecast` and
`Settings`.

Not covered yet: the HTTP layer itself (routing, `ValidationFilter`, the `Results.Problem` mapping).
Those would need a `WebApplicationFactory`, which today would boot the dev seeder or demand Key
Vault depending on the environment name — worth doing, but it needs a small `Program.cs`
testability change first.

### Writing tests for a new slice

**A slice is not done until it has tests.** Adding `Features/<Group>/<Slice>/` means adding the
matching folders in both test projects. Work down this list:

| The slice has… | Then add |
|---|---|
| a `<Slice>Validator.cs` | `FinanceOne.UnitTests/…/<Slice>ValidatorTests.cs` — one test per `RuleFor`, plus one `Valid_Command_Passes` |
| a `<Slice>Handler.cs` | `FinanceOne.UnitTests/…/<Slice>HandlerTests.cs` — one test per branch through `Handle` |
| a repository | `FinanceOne.IntegrationTests/…/<Slice>Tests.cs` — the happy path and each failure case from the slice's `README.md` |

Concretely, for each handler branch:

- **Every failure branch** asserts both the `ErrorCode` *and* that the write method was **not**
  called: `await _repository.DidNotReceive().Add(Arg.Any<X>(), Arg.Any<CancellationToken>())`.
  Asserting only the status code would pass even if the handler saved first and failed after.
- **The happy path** asserts on what was handed to the repository
  (`Arg.Is<Expense>(e => e.Name == … && e.Amount == …)`), not just that it returned success.
- **A handler that mutates a tracked entity** (every `Update<X>Handler`) asserts on the entity
  object itself, since that is the only visible effect before `SaveChanges`.

And in the integration test, cover whatever the *database* decides rather than repeating the unit
test: `decimal(18,2)` rounding, unique indexes, `Restrict` delete rules, `ORDER BY`, the
`DateOnly` ↔ `date` conversions, and `null`-vs-zero from `SumAsync`. If a test would pass against
an in-memory list, it belongs in the unit project.

### Test-writing conventions

- **`CancellationToken.None`**, not `TestContext.Current.CancellationToken` — this is xUnit v2.
- **Assert through `NewContext()`**, never through `Context`, when checking what was persisted.
  Reading back through the same context hits EF's change tracker and will pass even if nothing
  reached MySQL.
- **Arrange through the `Given*` helpers** on `IntegrationTest` (`GivenCategory`, `GivenBudget`,
  `GivenExpense`, `GivenIncome`, `GivenSavingGoal`, `GivenMonthlySaving`, `GivenDiscountCode`).
  Add a new one there when a new entity appears, and add its table to the delete list in
  `InitializeAsync` — **dependents before the rows they reference**, or the `Restrict` FKs reject
  the cleanup and every later test fails on leftover data.
- **Anything that reads the clock takes a `FakeTimeProvider`** (`Microsoft.Extensions.TimeProvider.Testing`),
  never `TimeProvider.System`. `GetSavingGoals`, `GetUpcomingPayments`, `GetDiscountCodes` and the
  date validators all do. Existing tests pin 2026-06-15 — mid-month, so recurrence days on both
  sides of "today" are expressible. `GetBudgets`/`GetBudgetById` do **not** — "used this month"
  sums every recurring expense in the category regardless of recurrence day, so there's no clock
  dependency to fake.
- **The `Income` entity clashes with the `Features.Income` namespace.** Inside
  `…Features.Income.*`, write `IncomeEntity` (a global using alias in each project's `Usings.cs`),
  the same way the API writes `Domain.Entites.Income`.
- **`Common/ServiceRegistrationTests.cs` covers DI for every slice automatically.** If it starts
  failing after you add one, the slice broke a naming convention — fix the name, not the test.

## Ground rules

1. A slice's files only depend on `Common/`, `Domain/`, `Configurations/`, and `Persistence/` —
   never on another slice's classes.
2. One class, one job. If a handler starts doing repository-shaped work or an endpoint starts
   doing handler-shaped work, split it out.
3. Naming conventions aren't just style — DI registration and endpoint discovery in this codebase
   rely on them.
4. Every slice's `README.md` is the source of truth for its behavior; keep it in sync with the
   implementation as slices get built out.
