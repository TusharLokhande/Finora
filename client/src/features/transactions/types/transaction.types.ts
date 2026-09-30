import type { z } from "zod";
import type { createTransactionSchema, transactionTypeSchema } from "../schemas/transaction.schema";
import type { PeriodKey } from "../constants/filters";

export type TransactionType = z.infer<typeof transactionTypeSchema>;

/** Mirrors backend TransactionDto (Application/Features/Transactions/Dto/TransactionDto.cs). */
export interface Transaction {
  id: string;
  type: TransactionType;
  txnDate: string;
  amount: number;
  accountId: string;
  toAccountId: string | null;
  categoryId: string | null;
  description: string | null;
  notes: string | null;
  accountName: string | null;
  toAccountName: string | null;
  categoryName: string | null;
  parentCategoryName: string | null;
}

/** Mirrors backend DescriptionSuggestionDto. */
export interface DescriptionSuggestion {
  description: string;
  type: TransactionType;
  categoryId: string | null;
  accountId: string;
}

export type CreateTransactionInput = z.infer<typeof createTransactionSchema>;

/** Toolbar filter state. Amounts are major units; `from`/`to` only apply to the custom period. */
export interface TransactionFilters {
  period: PeriodKey;
  from: string | null;
  to: string | null;
  accountIds: string[];
  categoryIds: string[];
  type: TransactionType | null;
  minAmount: number | null;
  maxAmount: number | null;
  search: string;
}
