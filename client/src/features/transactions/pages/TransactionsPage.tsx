import { useCallback, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Download, Plus, RefreshCw } from "lucide-react";
import { Button } from "@/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/ui/dropdown-menu";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { DEFAULT_FILTERS } from "../constants/filters";
import { focusQuickEntry } from "../constants/quickEntry";
import { useTransactions } from "../hooks/queries/useTransactions";
import { useExportTransactions } from "../hooks/mutations/useExportTransactions";
import { TransactionFiltersBar } from "../components/TransactionFiltersBar";
import { FilterChips } from "../components/FilterChips";
import { QuickEntryForm } from "../components/QuickEntryForm";
import { TransactionTable } from "../components/TransactionTable";
import { EditTransactionSheet } from "../components/EditTransactionSheet";
import type { Transaction, TransactionFilters } from "../types/transaction.types";

export function TransactionsPage() {
  // `?accountId=` lets other pages (e.g. a credit card row) deep-link into a filtered list.
  const [searchParams] = useSearchParams();
  const [filters, setFilters] = useState<TransactionFilters>(() => {
    const accountId = searchParams.get("accountId");
    return { ...DEFAULT_FILTERS, accountIds: accountId ? [accountId] : [] };
  });

  const { data, isLoading, isError, refetch, fetchNextPage, hasNextPage, isFetchingNextPage } = useTransactions(filters);
  const rows = data?.pages.flatMap((p) => p.items) ?? [];
  const totalCount = data?.pages[0]?.totalCount ?? 0;
  const isFiltered = JSON.stringify(filters) !== JSON.stringify(DEFAULT_FILTERS);

  const exportMutation = useExportTransactions();
  const [editing, setEditing] = useState<Transaction | undefined>();
  const loadMore = useCallback(() => fetchNextPage(), [fetchNextPage]);

  return (
    <div className="flex w-full flex-col gap-4 p-4 md:p-6">
      <PageBreadcrumb items={["Transactions"]} />
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="font-heading text-xl font-semibold text-foreground">Transactions</h1>
          <p className="text-sm text-muted-foreground">All your income, expenses and transfers.</p>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" disabled={exportMutation.isPending}>
                <Download /> Export
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onSelect={() => exportMutation.mutate(filters)}>Current filters (.xlsx)</DropdownMenuItem>
              <DropdownMenuItem onSelect={() => exportMutation.mutate(null)}>All transactions (.xlsx)</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <Button onClick={focusQuickEntry}>
            <Plus /> Add transaction
          </Button>
        </div>
      </div>

      <div className="flex flex-col gap-2">
        <TransactionFiltersBar value={filters} onChange={setFilters} />
        <FilterChips value={filters} onChange={setFilters} />
      </div>

      {isError && (
        <div role="alert" className="flex items-center justify-between gap-3 rounded-lg border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive">
          Couldn&apos;t load transactions.
          <Button variant="outline" size="sm" onClick={() => refetch()}>
            <RefreshCw /> Retry
          </Button>
        </div>
      )}

      <TransactionTable
        rows={rows}
        totalCount={totalCount}
        isLoading={isLoading}
        hasNextPage={hasNextPage}
        isFetchingNextPage={isFetchingNextPage}
        onLoadMore={loadMore}
        onEdit={setEditing}
        quickAdd={<QuickEntryForm />}
        emptyState={
          isError ? null : (
            <p className="px-3 py-8 text-center text-sm text-muted-foreground">
              {isFiltered ? (
                <>
                  No transactions match these filters.{" "}
                  <button type="button" className="text-primary hover:underline" onClick={() => setFilters(DEFAULT_FILTERS)}>
                    Clear filters
                  </button>
                </>
              ) : (
                "No transactions yet. Type an amount above and press Enter to log your first one."
              )}
            </p>
          )
        }
      />

      <EditTransactionSheet transaction={editing} onOpenChange={(open) => !open && setEditing(undefined)} />
    </div>
  );
}
