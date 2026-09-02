using FinanceOne.Api.Common;
using FinanceOne.Api.Features.Budgets;
using FinanceOne.Api.Features.Categories;
using FinanceOne.Api.Features.DiscountCodes;
using FinanceOne.Api.Features.Expenses;
using FinanceOne.Api.Features.Income;
using FinanceOne.Api.Features.SavingGoals;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<FinanceOneDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddFinanceOneServices();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

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

app.MapBudgetsEndpoints();
app.MapCategoriesEndpoints();
app.MapDiscountCodesEndpoints();
app.MapExpensesEndpoints();
app.MapIncomeEndpoints();
app.MapSavingGoalsEndpoints();

app.Run();
