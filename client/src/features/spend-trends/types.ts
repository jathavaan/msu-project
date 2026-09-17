/** Mirrors Features/SpendTrends/GetCategorySpendTrend/CategorySpendTrendVm.cs's MonthlySpendVm. */
export interface MonthlySpend {
  year: number
  month: number
  actual: number
}

/** Mirrors Features/SpendTrends/GetCategorySpendTrend/CategorySpendTrendVm.cs. */
export interface CategorySpendTrend {
  categoryId: string
  categoryName: string
  monthlyLimit: number | null
  months: MonthlySpend[]
}
