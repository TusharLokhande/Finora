import { useQuery } from "@tanstack/react-query";
import { transactionKeys } from "@/features/transactions";
import { accountKeys } from "@/features/accounts";
import {
  getBudgetsAtRisk,
  getDashboardSummary,
  getIncomeExpense,
  getTopCategories,
  getUpcomingDues,
} from "../../api/dashboard.api";

/**
 * Widgets derived from transactions key under the transactions prefix, and card dues under the
 * accounts prefix, so the existing invalidations (quick-add, edits, Pay) refresh them for free.
 */
export const dashboardKeys = {
  all: [...transactionKeys.all, "dashboard"] as const,
  summary: () => [...dashboardKeys.all, "summary"] as const,
  budgetsAtRisk: () => [...dashboardKeys.all, "budgets-at-risk"] as const,
  topCategories: (limit: number) => [...dashboardKeys.all, "top-categories", limit] as const,
  incomeExpense: (months: number) => [...dashboardKeys.all, "income-expense", months] as const,
  upcomingDues: () => [...accountKeys.all, "upcoming-dues"] as const,
};

export const useDashboardSummary = () => useQuery({ queryKey: dashboardKeys.summary(), queryFn: getDashboardSummary });

export const useBudgetsAtRisk = () => useQuery({ queryKey: dashboardKeys.budgetsAtRisk(), queryFn: getBudgetsAtRisk });

export const useTopCategories = (limit = 3) =>
  useQuery({ queryKey: dashboardKeys.topCategories(limit), queryFn: () => getTopCategories(limit) });

export const useIncomeExpense = (months = 6) =>
  useQuery({ queryKey: dashboardKeys.incomeExpense(months), queryFn: () => getIncomeExpense(months) });

export const useUpcomingDues = () => useQuery({ queryKey: dashboardKeys.upcomingDues(), queryFn: getUpcomingDues });
