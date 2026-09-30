import type { BudgetStatus } from "@/lib/usageTone";

/** Mirrors backend ReportSummaryDto. Amounts in minor units. */
export interface ReportSummary {
  from: string;
  to: string;
  priorFrom: string;
  priorTo: string;
  spent: number;
  income: number;
  netSavings: number;
  priorSpent: number;
  priorIncome: number;
  priorNetSavings: number;
  spentChangePercent: number | null;
  incomeChangePercent: number | null;
  netSavingsChangePercent: number | null;
  savingsRatePercent: number | null;
}

/** Mirrors backend CategorySpendDto. */
export interface CategorySpend {
  categoryId: string;
  name: string;
  icon: string | null;
  color: string | null;
  spent: number;
  sharePercent: number;
  hasSubcategories: boolean;
}

/** Mirrors backend CategoryBreakdownDto. */
export interface CategoryBreakdown {
  from: string;
  to: string;
  total: number;
  categories: CategorySpend[];
  other: { categoryCount: number; spent: number; sharePercent: number } | null;
}

/** Mirrors backend BudgetLineDto as returned by budget-vs-actual (always budgeted). */
export interface BudgetVsActualLine {
  categoryId: string;
  parentId: string | null;
  categoryName: string;
  parentCategoryName: string | null;
  icon: string | null;
  color: string | null;
  budgeted: number;
  spent: number;
  percentUsed: number | null;
  status: BudgetStatus;
}

/** Mirrors backend BudgetVsActualDto. */
export interface BudgetVsActual {
  from: string;
  to: string;
  monthsWithBudgets: number;
  totalBudgeted: number;
  totalSpent: number;
  lines: BudgetVsActualLine[];
}

/** Mirrors backend ReportRangeRequest: a named range or an explicit from/to. */
export type ReportRangeParams = { months: number } | { from: string; to: string };
