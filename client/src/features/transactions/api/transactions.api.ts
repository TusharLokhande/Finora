import { axiosClient } from "@/api/axiosClient";
import { toMinorUnits } from "@/lib/currency";
import type { PagedResult } from "@/types/pagination.types";
import { PAGE_SIZE, resolvePeriodRange } from "../constants/filters";
import type {
  CreateTransactionInput,
  DescriptionSuggestion,
  Transaction,
  TransactionFilters,
} from "../types/transaction.types";

/** Toolbar state → backend TransactionFilterRequest (minor units, resolved dates). */
function toFilterRequest(f: TransactionFilters) {
  return {
    ...resolvePeriodRange(f.period, f.from, f.to),
    accountIds: f.accountIds,
    categoryIds: f.categoryIds,
    type: f.type,
    minAmount: f.minAmount != null ? toMinorUnits(f.minAmount) : null,
    maxAmount: f.maxAmount != null ? toMinorUnits(f.maxAmount) : null,
    search: f.search.trim() || null,
  };
}

export async function searchTransactions(filters: TransactionFilters, page: number): Promise<PagedResult<Transaction>> {
  const res = await axiosClient.post<PagedResult<Transaction>>("/transactions/search", {
    page,
    pageSize: PAGE_SIZE,
    customFilter: toFilterRequest(filters),
  });
  return res.data;
}

export async function getRecentTransactions(limit: number): Promise<Transaction[]> {
  const res = await axiosClient.get<Transaction[]>("/transactions/recent", { params: { limit } });
  return res.data;
}

export async function getDescriptionSuggestions(q: string): Promise<DescriptionSuggestion[]> {
  const res = await axiosClient.get<DescriptionSuggestion[]>("/transactions/suggestions", { params: { q } });
  return res.data;
}

/** Downloads an .xlsx of the filtered transactions, or of everything when `filters` is null. */
export async function exportTransactions(filters: TransactionFilters | null): Promise<void> {
  const res = await axiosClient.post<Blob>("/transactions/export", filters ? toFilterRequest(filters) : {}, {
    responseType: "blob",
  });
  const url = URL.createObjectURL(res.data);
  const link = Object.assign(document.createElement("a"), {
    href: url,
    download: `transactions-${new Date().toISOString().slice(0, 10)}.xlsx`,
  });
  link.click();
  URL.revokeObjectURL(url);
}

function toRequestBody(payload: CreateTransactionInput) {
  const isTransfer = payload.type === "Transfer";
  return {
    type: payload.type,
    txnDate: payload.txnDate,
    amount: toMinorUnits(payload.amount),
    accountId: payload.accountId,
    toAccountId: isTransfer ? payload.toAccountId : null,
    categoryId: isTransfer ? null : payload.categoryId,
    description: payload.description || null,
    notes: payload.notes || null,
  };
}

export async function createTransaction(payload: CreateTransactionInput): Promise<Transaction> {
  const res = await axiosClient.post<Transaction>("/transactions", toRequestBody(payload));
  return res.data;
}

export async function updateTransaction(id: string, payload: CreateTransactionInput): Promise<Transaction> {
  const res = await axiosClient.put<Transaction>(`/transactions/${id}`, toRequestBody(payload));
  return res.data;
}

export async function deleteTransaction(id: string): Promise<Transaction> {
  const res = await axiosClient.delete<Transaction>(`/transactions/${id}`);
  return res.data;
}

export async function restoreTransaction(id: string): Promise<Transaction> {
  const res = await axiosClient.patch<Transaction>(`/transactions/${id}/restore`);
  return res.data;
}

export async function bulkDeleteTransactions(ids: string[]): Promise<number> {
  const res = await axiosClient.post<number>("/transactions/bulk-delete", { ids });
  return res.data;
}

export async function bulkRestoreTransactions(ids: string[]): Promise<number> {
  const res = await axiosClient.post<number>("/transactions/bulk-restore", { ids });
  return res.data;
}

export async function bulkRecategorizeTransactions(ids: string[], categoryId: string): Promise<number> {
  const res = await axiosClient.post<number>("/transactions/bulk-recategorize", { ids, categoryId });
  return res.data;
}
