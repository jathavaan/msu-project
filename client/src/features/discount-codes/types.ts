/** Mirrors Domain/Entites/DiscountCode.cs (returned as-is by this slice's queries) */
export interface DiscountCode {
  id: string
  storeName: string
  codeText: string | null
  codeImageUrl: string | null
  expiryDate: string
}

/** Mirrors Features/DiscountCodes/CreateDiscountCode/CreateDiscountCodeCommand.cs */
export interface CreateDiscountCodeRequest {
  storeName: string
  codeText: string | null
  codeImageUrl: string | null
  expiryDate: string
}

/** Mirrors Features/DiscountCodes/UpdateDiscountCode/UpdateDiscountCodeCommand.cs */
export interface UpdateDiscountCodeRequest extends CreateDiscountCodeRequest {
  id: string
}
