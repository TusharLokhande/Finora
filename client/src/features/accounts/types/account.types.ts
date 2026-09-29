import type { z } from "zod";
import type {
  accountTypeSchema,
  createAccountSchema,
  payCardSchema,
} from "../schemas/account.schema";

export type AccountType = z.infer<typeof accountTypeSchema>;

/** Mirrors backend AccountDto (Application/Features/Accounts/Dto/AccountDto.cs). */
export interface Account {
  id: string;
  name: string;
  type: AccountType;
  color: string | null;
  sortOrder: number;
  active: boolean;
  balance: number | null;
  outstanding: number | null;
  creditLimit: number | null;
  availableCredit: number | null;
  statementDay: number | null;
  dueDay: number | null;
  nextDueDate: string | null;
}

export type CreateAccountInput = z.infer<typeof createAccountSchema>;

export interface UpdateAccountInput {
  name: string;
  openingBalance: number;
  creditLimit: number | null;
  statementDay: number | null;
  dueDay: number | null;
}

export type PayCardInput = z.infer<typeof payCardSchema>;
