/** Mirrors backend MonthSummaryDto (Application/Features/Dashboard/Dto). Amounts in minor units. */
export interface MonthSummary {
  month: string;
  spent: number;
  income: number;
  netSavings: number;
  budgeted: number;
  budgetRemaining: number;
}

/** Mirrors backend DashboardSummaryDto. Percentages are 1-decimal; null when the base is zero. */
export interface DashboardSummary {
  current: MonthSummary;
  previous: MonthSummary;
  spentChangePercent: number | null;
  incomeChangePercent: number | null;
  netSavingsChangePercent: number | null;
  savingsRatePercent: number | null;
}

/** Mirrors backend BudgetLineDto (Application/Features/Budgets/Dto) — at-risk rows always have a budget. */
export interface BudgetUsage {
  categoryId: string;
  categoryName: string;
  parentCategoryName: string | null;
  icon: string | null;
  color: string | null;
  budgeted: number | null;
  spent: number;
  percentUsed: number | null;
  status: "Normal" | "Warning" | "Over";
}

/** Mirrors backend TopCategoryDto. */
export interface TopCategory {
  categoryId: string;
  name: string;
  icon: string | null;
  color: string | null;
  spent: number;
  sharePercent: number;
}

/** Mirrors backend UpcomingDueDto. */
export interface UpcomingDue {
  accountId: string;
  name: string;
  color: string | null;
  outstanding: number;
  creditLimit: number | null;
  dueDate: string;
  daysUntilDue: number;
}

/** Mirrors backend MonthlyTotalDto. */
export interface MonthlyTotal {
  month: string;
  income: number;
  expense: number;
  net: number;
}
