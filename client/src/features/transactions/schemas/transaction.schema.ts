import { z } from "zod";
import { toMajorUnits } from "@/lib/currency";
import type { CreateTransactionInput, Transaction } from "../types/transaction.types";

export const transactionTypeSchema = z.enum(["Expense", "Income", "Transfer"]);

export const createTransactionSchema = z
  .object({
    type: transactionTypeSchema,
    // Major units (e.g. rupees), as entered. Converted to minor units before sending.
    amount: z.number({ error: "Enter an amount" }).positive("Amount must be greater than zero"),
    accountId: z.string().min(1, "Choose an account"),
    toAccountId: z.string().nullable(),
    categoryId: z.string().nullable(),
    txnDate: z.string().min(1, "Date is required"),
    description: z.string().trim().max(200, "Description is too long").nullable(),
    notes: z.string().max(2000, "Notes are too long").nullable(),
  })
  .superRefine((data, ctx) => {
    if (data.type === "Transfer") {
      if (!data.toAccountId) {
        ctx.addIssue({ path: ["toAccountId"], code: "custom", message: "Choose a destination account" });
      } else if (data.toAccountId === data.accountId) {
        ctx.addIssue({ path: ["toAccountId"], code: "custom", message: "Pick a different destination account" });
      }
    } else if (!data.categoryId) {
      ctx.addIssue({ path: ["categoryId"], code: "custom", message: "Choose a category" });
    }
  });

/** An existing transaction as form values (major units), e.g. to edit one field and resubmit the rest. */
export function toFormValues(t: Transaction): CreateTransactionInput {
  return {
    type: t.type,
    amount: toMajorUnits(t.amount),
    accountId: t.accountId,
    toAccountId: t.toAccountId,
    categoryId: t.categoryId,
    txnDate: t.txnDate,
    description: t.description,
    notes: t.notes,
  };
}
