import { axiosClient } from "@/api/axiosClient";
import type { MonthlyTotal } from "@/features/dashboard";
import type { BudgetVsActual, CategoryBreakdown, ReportRangeParams, ReportSummary } from "../types/report.types";

export const TOP_CATEGORIES = 5;

export async function getReportSummary(params: ReportRangeParams): Promise<ReportSummary> {
  return (await axiosClient.get<ReportSummary>("/reports/summary", { params })).data;
}

export async function getCategoryBreakdown(params: ReportRangeParams): Promise<CategoryBreakdown> {
  return (await axiosClient.get<CategoryBreakdown>("/reports/by-category", { params: { ...params, top: TOP_CATEGORIES } })).data;
}

export async function getSubcategoryBreakdown(categoryId: string, params: ReportRangeParams): Promise<CategoryBreakdown> {
  return (await axiosClient.get<CategoryBreakdown>(`/reports/by-category/${categoryId}/subcategories`, { params })).data;
}

export async function getIncomeVsExpense(params: ReportRangeParams): Promise<MonthlyTotal[]> {
  return (await axiosClient.get<MonthlyTotal[]>("/reports/income-vs-expense", { params })).data;
}

export async function getBudgetVsActual(params: ReportRangeParams): Promise<BudgetVsActual> {
  return (await axiosClient.get<BudgetVsActual>("/reports/budget-vs-actual", { params })).data;
}

/** Downloads the .xlsx, using the server's file name (it carries the resolved dates). */
export async function exportReport(params: ReportRangeParams): Promise<void> {
  const res = await axiosClient.get<Blob>("/reports/export", { params, responseType: "blob" });
  const disposition = String(res.headers["content-disposition"] ?? "");
  const name = /filename="?([^";]+)"?/.exec(disposition)?.[1] ?? "report.xlsx";
  const url = URL.createObjectURL(res.data);
  Object.assign(document.createElement("a"), { href: url, download: name }).click();
  URL.revokeObjectURL(url);
}
