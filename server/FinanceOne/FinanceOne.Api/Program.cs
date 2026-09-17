using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using FinanceOne.Api.Common;
using FinanceOne.Api.Common.BlobStorage;
using FinanceOne.Api.Features.BalanceForecast;
using FinanceOne.Api.Features.Budgets;
using FinanceOne.Api.Features.Categories;
using FinanceOne.Api.Features.CategorizationRules;
using FinanceOne.Api.Features.DiscountCodes;
using FinanceOne.Api.Features.Expenses;
using FinanceOne.Api.Features.Income;
using FinanceOne.Api.Features.MonthlySavings;
using FinanceOne.Api.Features.SavingGoals;
using FinanceOne.Api.Features.Transactions;
using FinanceOne.Api.Features.UpcomingPayments;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;

var builder = WebApplication.CreateBuilder(args);

// Replaces the default Microsoft.Extensions.Logging console provider. Config (levels/overrides)
// comes from the "Serilog" section in appsettings — see server/FinanceOne/CLAUDE.md's Logging
// section. Development gets plain readable console output; everywhere else gets one-line-per-
// event JSON (CompactJsonFormatter) so AKS pod stdout is directly queryable once something
// scrapes it, without needing a dedicated telemetry backend.
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName();

    loggerConfiguration.WriteTo.Console(context.HostingEnvironment.IsDevelopment()
        ? new MessageTemplateTextFormatter(
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
        : new CompactJsonFormatter());
},
    // Lets the same log events (including GlobalExceptionHandler's exception log) also reach the
    // OpenTelemetry logging provider registered by UseAzureMonitor below, instead of only going to
    // Serilog's own sinks. Stdout logging above is unaffected either way — this only adds a second
    // destination.
    writeToProviders: true);

// Outside Development, secrets come from Azure Key Vault instead of appsettings/env vars.
// Secret names use "--" in place of ":" (e.g. a secret "Foo--Bar" becomes config key
// "Foo:Bar"). DefaultAzureCredential resolves via Workload Identity when running in AKS, or
// the developer's `az login` session when running locally against a non-Development
// environment. Nothing currently lives in the vault — the DB connection uses Azure AD auth
// (below), not a stored password — but this stays wired up for whatever secrets come next.
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["KeyVault:Uri"]
        ?? throw new InvalidOperationException("KeyVault:Uri must be configured outside Development.");
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

// Application Insights via the Azure Monitor OpenTelemetry Distro (issue #50): ASP.NET Core
// request traces, outgoing HttpClient dependency calls, and (via writeToProviders above) the same
// ILogger events already going to stdout, all in one exporter. Skipped entirely rather than
// pointed at a placeholder when no connection string is configured (Development has none), so
// nothing tries to phone home locally. `ApplicationInsights:ConnectionString` outside Development
// resolves from Key Vault (`ApplicationInsights--ConnectionString`), same as every other secret —
// see server/FinanceOne/CLAUDE.md's Configuration & secrets section.
//
// MySql.EntityFrameworkCore's underlying driver (Oracle's MySql.Data) has no OpenTelemetry
// ActivitySource of its own, so MySQL calls won't show up as dependency spans the way HttpClient
// calls do — nothing to wire up for that today.
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
        options.SamplingRatio = 1.0f; // 100% capture, no sampling — see issue #50.
    });
}

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<FinanceOneDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MySql")!;

    if (!builder.Environment.IsDevelopment())
    {
        // financeone-sqlserver has aad_auth_only=ON — no MySQL password auth exists at all.
        // Use a short-lived Azure AD access token as the password instead, acquired via the
        // same Workload Identity used for Key Vault (financeone-uami, mapped to the
        // 'financeone-uami' AAD MySQL user via CREATE AADUSER). DefaultAzureCredential caches
        // tokens internally and only re-issues near expiry, so the connection string stays
        // stable across most requests — ADO.NET's connection pooling (keyed by connection
        // string) still works.
        var token = new DefaultAzureCredential()
            .GetToken(new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));
        connectionString += $";Pwd={token.Token}";
    }

    options.UseMySQL(connectionString);
});

// Same production/local split as the MySQL connection above: outside Development the account is
// reached passwordlessly via Workload Identity (financeone-uami has Storage Blob Data Contributor
// on financeoneappstorage — see infra/role-assignments.bicep); Development points at the Azurite
// emulator via a connection string (docker-compose's ConnectionStrings__BlobStorage, or a
// user-secret for a bare `dotnet run`).
builder.Services.AddSingleton(_ =>
{
    if (builder.Environment.IsDevelopment())
    {
        var connectionString = builder.Configuration.GetConnectionString("BlobStorage")!;
        return new BlobServiceClient(connectionString);
    }

    var accountUrl = builder.Configuration["BlobStorage:AccountUrl"]
        ?? throw new InvalidOperationException("BlobStorage:AccountUrl must be configured outside Development.");
    return new BlobServiceClient(new Uri(accountUrl), new DefaultAzureCredential());
});
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

builder.Services.AddFinanceOneServices();

// Backs the two probe endpoints below. Only the database check is tagged "ready": liveness must
// stay independent of the database, or a transient MySQL outage would make kubelet restart every
// pod at once and turn a recoverable blip into an outage.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceOneDbContext>("database", tags: ["ready"]);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Migration mode: `dotnet FinanceOne.Api.dll --migrate` applies pending migrations and exits,
// without starting Kestrel or mapping any routes. Same image as the running app, so migrations
// always match the code they ship with. Run as a one-off k8s Job before each deploy
// (see k8s/server-migration-job.yaml) rather than from every app pod, to avoid replicas racing
// to apply the same migration.
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceOneDbContext>();
    await db.Database.MigrateAsync();
    return;
}

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "FinanceOne API v1");
    options.RoutePrefix = "api";
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceOneDbContext>();
    await FinanceOneDbSeeder.SeedAsync(db);

    // In production the containers already exist (created by infra/modules/storage-app.bicep);
    // Azurite starts with none, so Development creates them itself on boot.
    var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
    foreach (var container in new[] { BlobContainers.Coupons, BlobContainers.Exports, BlobContainers.StagedCsv })
    {
        await blobServiceClient.GetBlobContainerClient(container).CreateIfNotExistsAsync();
    }
}

// Kubernetes probes (see k8s/server-deployment.yaml).
//
//   /health/ready  runs the database check — the Service only sends traffic to a pod that can
//                  actually reach MySQL. This is what makes `kubectl rollout status` meaningful:
//                  without it a rollout reports success the moment the process starts.
//   /health/live   runs no checks at all (Predicate = false); it answers "is this process still
//                  responding", which is the only question a restart can fix.
//
// Registered as terminal middleware here rather than as routed endpoints, deliberately, so they
// sit ahead of UseHttpsRedirection and the request logging:
//   - probes reach the container over plain HTTP, and if an HTTPS port were ever configured a
//     routed endpoint would answer them with a 307 that kubelet counts as success — silently
//     disabling both probes;
//   - a probe every few seconds per pod would otherwise fill the logs with request-summary lines.
app.UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.UseHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

// Pushes the per-request TraceIdentifier onto Serilog's ambient LogContext, so every log line
// written while handling this request (handler logs, the exception log below, and the request
// summary line from UseSerilogRequestLogging) carries the same RequestId and can be grepped/
// filtered together. Must wrap everything downstream, so it's the outermost middleware.
app.Use(async (context, next) =>
{
    using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
    {
        await next();
    }
});

// One summary log line per request (method, path, status code, elapsed ms). Placed before
// UseExceptionHandler so it still runs — and logs the resulting status code — even when a
// downstream handler throws.
app.UseSerilogRequestLogging();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Client");

app.MapBalanceForecastEndpoints();
app.MapBudgetsEndpoints();
app.MapCategoriesEndpoints();
app.MapCategorizationRulesEndpoints();
app.MapDiscountCodesEndpoints();
app.MapExpensesEndpoints();
app.MapIncomeEndpoints();
app.MapMonthlySavingsEndpoints();
app.MapSavingGoalsEndpoints();
app.MapTransactionsEndpoints();
app.MapUpcomingPaymentsEndpoints();

app.Run();
