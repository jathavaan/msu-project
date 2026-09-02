import type { CategoryType } from '../../lib/types'

/** Mirrors Features/UpcomingPayments/GetUpcomingPayments/UpcomingPaymentVm.cs — read-only, no id. */
export interface UpcomingPayment {
  date: string
  name: string
  amount: number
  type: CategoryType
}
