import { formatDate } from '../../lib/formatters'

/** Shared by SavingGoalCard and SavingGoalProjectionChart so the two stay worded identically. */
export function formatReachDateMessage(reachDate: string | null): string {
  return reachDate ? `On track to reach this goal by ${formatDate(reachDate)}` : 'Not reachable at the current rate'
}
