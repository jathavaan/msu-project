using Azure.Identity;
using FinanceOne.Api.Common;
using FinanceOne.Api.Features.Budgets;
using FinanceOne.Api.Features.Categories;
using FinanceOne.Api.Features.DiscountCodes;
using FinanceOne.Api.Features.Expenses;
using FinanceOne.Api.Features.Income;
using FinanceOne.Api.Features.SavingGoals;
using FinanceOne.Api.Features.UpcomingPayments;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Outside Development, secrets (e.g. ConnectionStrings:MySql) come from Azure Key Vault
// instead of appsettings/env vars. Secret names use "--" in place of ":" (e.g. the secret
// "ConnectionStrings--MySql" becomes config key "ConnectionStrings:MySql").
// DefaultAzureCredential resolves via Workload Identity when running in AKS, or the
// developer's `az login` session when running locally against a non-Development environment.
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = builder.Configuration["KeyVault:Uri"]
        ?? throw new InvalidOperationException("KeyVault:Uri must be configured outside Development.");
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
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
    options.UseMySQL(builder.Configuration.GetConnectionString("MySql")!));

builder.Services.AddFinanceOneServices();

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
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Client");

app.MapBudgetsEndpoints();
app.MapCategoriesEndpoints();
app.MapDiscountCodesEndpoints();
app.MapExpensesEndpoints();
app.MapIncomeEndpoints();
app.MapSavingGoalsEndpoints();
app.MapUpcomingPaymentsEndpoints();

app.Run();
