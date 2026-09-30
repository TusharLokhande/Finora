import { axiosClient } from "@/api/axiosClient";
import type { BudgetUsage, DashboardSummary, MonthlyTotal, TopCategory, UpcomingDue } from "../types/dashboard.types";

export async function getDashboardSummary(): Promise<DashboardSummary> {
  const res = await axiosClient.get<DashboardSummary>("/dashboard/summary");
  return res.data;
}

export async function getBudgetsAtRisk(): Promise<BudgetUsage[]> {
  const res = await axiosClient.get<BudgetUsage[]>("/dashboard/budgets-at-risk");
  return res.data;
}

export async function getTopCategories(limit: number): Promise<TopCategory[]> {
  const res = await axiosClient.get<TopCategory[]>("/dashboard/top-categories", { params: { limit } });
  return res.data;
}

export async function getUpcomingDues(): Promise<UpcomingDue[]> {
  const res = await axiosClient.get<UpcomingDue[]>("/dashboard/upcoming-dues");
  return res.data;
}

export async function getIncomeExpense(months: number): Promise<MonthlyTotal[]> {
  const res = await axiosClient.get<MonthlyTotal[]>("/dashboard/income-expense", { params: { months } });
  return res.data;
}
