import { axiosClient } from "@/api/axiosClient";
import type { BudgetMonth, SetBudgetInput } from "../types/budget.types";

export async function getBudgetMonth(month: string): Promise<BudgetMonth> {
  const res = await axiosClient.get<BudgetMonth>("/budgets", { params: { month } });
  return res.data;
}

export async function hasAnyBudgets(month: string): Promise<boolean> {
  const res = await axiosClient.get<boolean>("/budgets/has-any", { params: { month } });
  return res.data;
}

export async function setBudget(payload: SetBudgetInput): Promise<BudgetMonth> {
  const res = await axiosClient.put<BudgetMonth>("/budgets", payload);
  return res.data;
}

export async function copyBudgetsFromPrevious(month: string): Promise<number> {
  const res = await axiosClient.post<number>("/budgets/copy-from-previous", null, { params: { month } });
  return res.data;
}
