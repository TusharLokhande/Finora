import type { BudgetLine, BudgetStatus } from "../types/budget.types";

/**
 * Mirror of backend BudgetMath, used ONLY to paint an optimistic edit before the server answers;
 * the saved month from the server replaces it. Same exact-integer thresholds: 80% warning, 100% over.
 */
export function withBudget(line: BudgetLine, budgeted: number): BudgetLine {
  const { spent } = line;
  const status: BudgetStatus =
    budgeted === 0
      ? spent > 0 ? "Over" : "Normal"
      : spent * 100 >= budgeted * 100
        ? "Over"
        : spent * 100 >= budgeted * 80
          ? "Warning"
          : "Normal";

  return {
    ...line,
    budgeted,
    percentUsed: budgeted === 0 ? null : Math.round((spent * 1000) / budgeted) / 10,
    status,
  };
}
