/** Mirrors Features/MonthlySavings/GetMonthlySavings/MonthlySavingVm.cs */
export interface MonthlySaving {
  id: string
  name: string
  amount: number
  savingGoalId: string
  savingGoalName: string
  recurrenceDay: number
}

/** Mirrors Features/MonthlySavings/CreateMonthlySaving/CreateMonthlySavingCommand.cs */
export interface CreateMonthlySavingRequest {
  name: string
  amount: number
  savingGoalId: string
  recurrenceDay: number
}

/** Mirrors Features/MonthlySavings/UpdateMonthlySaving/UpdateMonthlySavingCommand.cs */
export interface UpdateMonthlySavingRequest extends CreateMonthlySavingRequest {
  id: string
}
