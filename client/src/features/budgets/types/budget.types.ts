import type { z } from "zod";
import type { budgetStatusSchema } from "../schemas/budget.schema";

export type BudgetStatus = z.infer<typeof budgetStatusSchema>;

/** Mirrors backend BudgetLineDto (Application/Features/Budgets/Dto). Amounts in minor units. */
export interface BudgetLine {
  categoryId: string;
  parentId: string | null;
  categoryName: string;
  parentCategoryName: string | null;
  icon: string | null;
  color: string | null;
  sortOrder: number;
  active: boolean;
  budgeted: number | null;
  spent: number;
  percentUsed: number | null;
  status: BudgetStatus;
}

/** Mirrors backend BudgetMonthDto. `month` is "yyyy-MM-01". */
export interface BudgetMonth {
  month: string;
  totalBudgeted: number;
  totalSpent: number;
  remaining: number;
  lines: BudgetLine[];
}

/** Mirrors backend SetBudgetRequest; amount in minor units. */
export interface SetBudgetInput {
  categoryId: string;
  month: string;
  amount: number;
}

export type BudgetSort = "category" | "mostUsed";
