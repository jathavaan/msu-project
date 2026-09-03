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
FinanceOne.Test/
  Features/<Group>/<Slice>/<Slice>Tests.cs   Mirrors the Features/ tree 1:1
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

## Testing

`FinanceOne.Test/` mirrors `Features/` 1:1: `Features/Budgets/CreateBudget/CreateBudgetTests.cs`
next to the slice it tests.

- Integration tests, not handler-with-mocked-repository unit tests — since repositories talk to
  EF Core directly, the meaningful thing to verify is the slice's actual query/persistence
  behavior against a real database engine.
- Use **Testcontainers** (`Testcontainers.MySql`) to spin up a real MySQL container per test
  run, migrated with the same `FinanceOneDbContext`/migrations used in production. This catches
  MySQL-specific behavior (decimal precision, unique indexes, cascade/restrict delete rules)
  that an in-memory or SQLite provider would silently miss.
- One test class per slice, covering: the happy path, each documented failure case from that
  slice's `README.md` (404/409/etc.), and edge cases specific to its business rules.

Add `Testcontainers.MySql` and a test runner (`xunit` + `Microsoft.AspNetCore.Mvc.Testing`, or
whatever the team settles on) to `FinanceOne.Test.csproj` — not yet referenced.

## Ground rules

1. A slice's files only depend on `Common/`, `Domain/`, `Configurations/`, and `Persistence/` —
   never on another slice's classes.
2. One class, one job. If a handler starts doing repository-shaped work or an endpoint starts
   doing handler-shaped work, split it out.
3. Naming conventions aren't just style — DI registration and endpoint discovery in this codebase
   rely on them.
4. Every slice's `README.md` is the source of truth for its behavior; keep it in sync with the
   implementation as slices get built out.
