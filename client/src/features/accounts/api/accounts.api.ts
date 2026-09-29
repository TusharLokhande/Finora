import { axiosClient } from "@/api/axiosClient";
import { toMinorUnits } from "@/lib/currency";
import type { Account, CreateAccountInput, PayCardInput, UpdateAccountInput } from "../types/account.types";

export async function getAccounts(): Promise<Account[]> {
  const res = await axiosClient.get<Account[]>("/accounts");
  return res.data;
}

export async function createAccount(payload: CreateAccountInput): Promise<Account> {
  const res = await axiosClient.post<Account>("/accounts", {
    name: payload.name,
    type: payload.type,
    openingBalance: toMinorUnits(payload.openingBalance),
    creditLimit: payload.creditLimit != null ? toMinorUnits(payload.creditLimit) : null,
    statementDay: payload.statementDay,
    dueDay: payload.dueDay,
  });
  return res.data;
}

export async function updateAccount(id: string, payload: UpdateAccountInput): Promise<Account> {
  const res = await axiosClient.put<Account>(`/accounts/${id}`, {
    name: payload.name,
    openingBalance: toMinorUnits(payload.openingBalance),
    creditLimit: payload.creditLimit != null ? toMinorUnits(payload.creditLimit) : null,
    statementDay: payload.statementDay,
    dueDay: payload.dueDay,
  });
  return res.data;
}

export async function archiveAccount(id: string): Promise<Account> {
  const res = await axiosClient.patch<Account>(`/accounts/${id}/archive`);
  return res.data;
}

export async function restoreAccount(id: string): Promise<Account> {
  const res = await axiosClient.patch<Account>(`/accounts/${id}/restore`);
  return res.data;
}

export async function reorderAccounts(accountIds: string[]): Promise<Account[]> {
  const res = await axiosClient.patch<Account[]>("/accounts/reorder", { accountIds });
  return res.data;
}

export async function payCard(cardAccountId: string, payload: PayCardInput): Promise<Account> {
  const res = await axiosClient.post<Account>(`/accounts/${cardAccountId}/pay`, {
    sourceAccountId: payload.sourceAccountId,
    amount: toMinorUnits(payload.amount),
    date: payload.date,
    note: payload.note,
  });
  return res.data;
}
