/** Mirrors Features/SavingGoals/GetSavingGoals/SavingGoalVm.cs */
export interface SavingGoal {
  id: string
  name: string
  targetAmount: number
  targetDate: string
  amountSaved: number
  amountRemaining: number
  daysRemaining: number
}

/** Mirrors Features/SavingGoals/CreateSavingGoal/CreateSavingGoalCommand.cs */
export interface CreateSavingGoalRequest {
  name: string
  targetAmount: number
  targetDate: string
}

/** Mirrors Features/SavingGoals/UpdateSavingGoal/UpdateSavingGoalCommand.cs — CurrentAmount adjusts AmountSaved. */
export interface UpdateSavingGoalRequest {
  id: string
  name: string
  targetAmount: number
  targetDate: string
  currentAmount: number
}
