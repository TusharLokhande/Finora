import { z } from "zod";

export const accountTypeSchema = z.enum(["Bank", "Cash", "Wallet", "CreditCard"]);

const baseAccountSchema = z.object({
  name: z.string().trim().min(1, "Name is required").max(200, "Name is too long"),
  type: accountTypeSchema,
  // Major units (e.g. rupees), as entered in the form. Converted to minor units before sending.
  openingBalance: z.number().min(0, "Must be zero or more"),
  creditLimit: z.number().positive("Credit limit must be greater than zero").nullable(),
  statementDay: z.number().int().min(1).max(28).nullable(),
  dueDay: z.number().int().min(1).max(28).nullable(),
});

export const createAccountSchema = baseAccountSchema.superRefine((data, ctx) => {
  if (data.type !== "CreditCard") return;

  if (data.creditLimit == null) {
    ctx.addIssue({ path: ["creditLimit"], code: "custom", message: "Credit limit is required for a credit card" });
  }
  if (data.statementDay == null) {
    ctx.addIssue({ path: ["statementDay"], code: "custom", message: "Statement day is required for a credit card" });
  }
  if (data.dueDay == null) {
    ctx.addIssue({ path: ["dueDay"], code: "custom", message: "Due day is required for a credit card" });
  }
});

export type CreateAccountFormValues = z.infer<typeof createAccountSchema>;

export const payCardSchema = z.object({
  sourceAccountId: z.uuid("Choose a source account"),
  amount: z.number().positive("Amount must be greater than zero"),
  date: z.string().min(1, "Date is required"),
  note: z.string().max(2000).nullable(),
});
