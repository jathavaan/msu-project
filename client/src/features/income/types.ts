/** Mirrors Features/Income/GetIncomes/IncomeVm.cs */
export interface Income {
  id: string
  name: string
  amount: number
  categoryId: string
  categoryName: string
  recurrenceDay: number
}

/** Mirrors Features/Income/CreateIncome/CreateIncomeCommand.cs */
export interface CreateIncomeRequest {
  name: string
  amount: number
  categoryId: string
  recurrenceDay: number
}

/** Mirrors Features/Income/UpdateIncome/UpdateIncomeCommand.cs */
export interface UpdateIncomeRequest extends CreateIncomeRequest {
  id: string
}
