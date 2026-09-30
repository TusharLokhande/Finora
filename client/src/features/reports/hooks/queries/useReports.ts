import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { transactionKeys } from "@/features/transactions";
import {
  getBudgetVsActual,
  getCategoryBreakdown,
  getIncomeVsExpense,
  getReportSummary,
  getSubcategoryBreakdown,
} from "../../api/reports.api";
import type { ReportRangeParams } from "../../types/report.types";

/** Reports are derived from transactions, so they key under that prefix and refresh after any write. */
export const reportKeys = {
  all: [...transactionKeys.all, "reports"] as const,
  section: (section: string, params: ReportRangeParams | null) => [...reportKeys.all, section, params] as const,
  subcategories: (categoryId: string, params: ReportRangeParams | null) =>
    [...reportKeys.all, "subcategories", categoryId, params] as const,
};

/** Every section waits while a custom range is half-filled, and keeps its last data while the range changes. */
function useSection<T>(section: string, params: ReportRangeParams | null, fetch: (p: ReportRangeParams) => Promise<T>) {
  return useQuery({
    queryKey: reportKeys.section(section, params),
    queryFn: () => fetch(params!),
    enabled: params !== null,
    placeholderData: keepPreviousData,
  });
}

export const useReportSummary = (p: ReportRangeParams | null) => useSection("summary", p, getReportSummary);
export const useCategoryBreakdown = (p: ReportRangeParams | null) => useSection("by-category", p, getCategoryBreakdown);
export const useReportIncomeVsExpense = (p: ReportRangeParams | null) => useSection("income-vs-expense", p, getIncomeVsExpense);
export const useBudgetVsActual = (p: ReportRangeParams | null) => useSection("budget-vs-actual", p, getBudgetVsActual);

export function useSubcategoryBreakdown(categoryId: string, params: ReportRangeParams | null, enabled: boolean) {
  return useQuery({
    queryKey: reportKeys.subcategories(categoryId, params),
    queryFn: () => getSubcategoryBreakdown(categoryId, params!),
    enabled: enabled && params !== null,
  });
}
