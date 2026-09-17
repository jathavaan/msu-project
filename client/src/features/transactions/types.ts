/** Mirrors Features/Transactions/GetTransactions/TransactionVm.cs. */
export interface Transaction {
  id: string
  date: string
  description: string
  amount: number
  categoryId: string | null
  categoryName: string | null
}
