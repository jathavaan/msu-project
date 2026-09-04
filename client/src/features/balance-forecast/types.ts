/** Mirrors Features/BalanceForecast/GetBalanceForecast/BalanceForecastPointVm.cs's BalanceEntryVm. */
export interface BalanceEntry {
  name: string
  categoryName: string
  amount: number
}

/** Mirrors Features/BalanceForecast/GetBalanceForecast/BalanceForecastPointVm.cs. */
export interface BalanceForecastPoint {
  day: number
  balance: number
  incomes: BalanceEntry[]
  expenses: BalanceEntry[]
}
