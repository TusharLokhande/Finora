import { z } from "zod";

export const budgetStatusSchema = z.enum(["Normal", "Warning", "Over"]);

/** The inline amount editor, in major units as typed. */
export const budgetAmountSchema = z
  .number({ error: "Enter an amount" })
  .nonnegative("Budget can't be negative")
  .finite();
