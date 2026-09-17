/** Mirrors Features/SavingGoals/GetSavingGoals/SavingGoalVm.cs */
export interface SavingGoal {
  id: string
  name: string
  targetAmount: number
  targetDate: string
  amountSaved: number
  amountRemaining: number
  daysRemaining: number
  /** Sum of Amount across every Monthly Saving linked to this goal (0 if none). */
  monthlyContribution: number
  /** Annual percentage, e.g. 4.5 for 4.5%. Null when the goal has no rate set. */
  interestRate: number | null
  /** Relative path to the uploaded image (see UploadSavingGoalImage), e.g. `/api/saving-goals/{id}/image`. Null until one is uploaded. */
  imageUrl: string | null
}

/** Mirrors Features/SavingGoals/CreateSavingGoal/CreateSavingGoalCommand.cs */
export interface CreateSavingGoalRequest {
  name: string
  targetAmount: number
  targetDate: string
  interestRate: number | null
}

/** Mirrors Features/SavingGoals/UpdateSavingGoal/UpdateSavingGoalCommand.cs — CurrentAmount adjusts AmountSaved. */
export interface UpdateSavingGoalRequest {
  id: string
  name: string
  targetAmount: number
  targetDate: string
  currentAmount: number
  interestRate: number | null
}

/** Mirrors Features/SavingGoals/GetSavingGoalProjection/SavingGoalProjectionVm.cs's SavingGoalProjectionPointVm. */
export interface SavingGoalProjectionPoint {
  month: number
  date: string
  balance: number
}

/** Mirrors Features/SavingGoals/GetSavingGoalProjection/SavingGoalProjectionVm.cs. `reachDate` is
 * null when the goal is never projected to reach its target — see that slice's README. */
export interface SavingGoalProjection {
  points: SavingGoalProjectionPoint[]
  reachDate: string | null
}

/** Mirrors Features/SavingGoals/GetSavingGoalsProjection/SavingGoalsProjectionPointVm.cs — the
 * combined balance across every saving goal, for the "total projected savings" chart. */
export interface SavingGoalsProjectionPoint {
  month: number
  date: string
  totalBalance: number
}
