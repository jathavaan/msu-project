/** Mirrors Features/Budgets/GetBudgets/BudgetVm.cs */
export interface Budget {
  id: string
  categoryId: string
  categoryName: string
  monthlyLimit: number
  usedThisMonth: number
}

/** Mirrors Features/Budgets/CreateBudget/CreateBudgetCommand.cs */
export interface CreateBudgetRequest {
  categoryId: string
  monthlyLimit: number
}

/** Mirrors Features/Budgets/UpdateBudget/UpdateBudgetCommand.cs — category is fixed at creation. */
export interface UpdateBudgetRequest {
  id: string
  monthlyLimit: number
}
