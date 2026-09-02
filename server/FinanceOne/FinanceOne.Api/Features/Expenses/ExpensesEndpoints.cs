using FinanceOne.Api.Features.Expenses.CreateExpense;
using FinanceOne.Api.Features.Expenses.DeleteExpense;
using FinanceOne.Api.Features.Expenses.GetExpenseById;
using FinanceOne.Api.Features.Expenses.GetExpenses;
using FinanceOne.Api.Features.Expenses.UpdateExpense;

namespace FinanceOne.Api.Features.Expenses;

public static class ExpensesEndpoints
{
    public static void MapExpensesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/expenses").WithTags("Expenses");

        group.MapCreateExpense();
        group.MapGetExpenses();
        group.MapGetExpenseById();
        group.MapUpdateExpense();
        group.MapDeleteExpense();
    }
}
