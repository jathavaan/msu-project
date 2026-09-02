/** Mirrors the backend's Common/Response.cs envelope. */
export interface ApiEnvelope<T> {
  result: T | null
  errorCode: number | null
  errorMessage: string | null
}

/** Mirrors Domain/Enums/CategoryType.cs. Not a TS enum — erasableSyntaxOnly forbids those. */
export type CategoryType = 'Income' | 'Expense'
