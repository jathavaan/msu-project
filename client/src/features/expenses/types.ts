/** Mirrors Features/Expenses/GetExpenses/ExpenseVm.cs */
export interface Expense {
  id: string
  name: string
  amount: number
  categoryId: string
  categoryName: string
  recurrenceDay: number
}

/** Mirrors Features/Expenses/CreateExpense/CreateExpenseCommand.cs */
export interface CreateExpenseRequest {
  name: string
  amount: number
  categoryId: string
  recurrenceDay: number
}

/** Mirrors Features/Expenses/UpdateExpense/UpdateExpenseCommand.cs */
export interface UpdateExpenseRequest extends CreateExpenseRequest {
  id: string
}
