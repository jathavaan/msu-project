/** Mirrors the backend's Common/Response.cs envelope. */
export interface ApiEnvelope<T> {
  result: T | null
  errorCode: number | null
  errorMessage: string | null
}

/**
 * Mirrors Domain/Enums/CategoryType.cs. Not a TS enum — erasableSyntaxOnly forbids those, and more
 * importantly the wire format isn't a string anyway: System.Text.Json serializes C# enums as
 * their numeric value by default (no JsonStringEnumConverter is registered in Program.cs), so
 * every request/response carries 0 | 1 here, matching CategoryType.cs's declaration order.
 */
export const CategoryType = {
  Income: 0,
  Expense: 1,
} as const

export type CategoryType = (typeof CategoryType)[keyof typeof CategoryType]
