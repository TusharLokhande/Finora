import { Link } from "react-router-dom";
import { RefreshCw } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/ui/button";
import { Skeleton } from "@/ui/skeleton";
import { useRecentTransactions } from "../hooks/queries/useTransactions";
import { useLookupOptions } from "../hooks/queries/useLookupOptions";
import { ROW_HEIGHT, formatShortDate } from "../constants/ledger";
import { AmountText, CategoryLabel } from "./TransactionCells";

// The ledger's cells and row height, minus select/account/actions: date · description · category · amount.
const GRID = "grid items-center gap-x-3 px-4 grid-cols-[3.5rem_minmax(0,1fr)_auto] sm:grid-cols-[3.5rem_minmax(0,1fr)_minmax(0,9rem)_auto]";

/** Read-only newest transactions, styled like the Transactions ledger. */
export function RecentTransactionsCard({ limit = 8 }: { limit?: number }) {
  const { data, isLoading, isError, refetch } = useRecentTransactions(limit);
  const { categoryById } = useLookupOptions();

  return (
    <section className="rounded-xl border border-border bg-card">
      <div className="flex items-center justify-between px-4 pt-4 pb-2">
        <h2 className="text-sm font-medium text-foreground">Recent transactions</h2>
        <Link to="/transactions" className="text-xs font-medium text-primary hover:underline">
          View all
        </Link>
      </div>

      {isError ? (
        <div className="flex items-center justify-between gap-2 px-4 pb-4 text-sm text-destructive">
          Couldn&apos;t load recent transactions.
          <Button variant="outline" size="sm" onClick={() => refetch()}>
            <RefreshCw /> Retry
          </Button>
        </div>
      ) : (
        <div className="pb-2 text-sm">
          {isLoading
            ? Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className={cn(GRID, ROW_HEIGHT)}>
                  <Skeleton className="h-3.5 w-10" />
                  <Skeleton className="h-3.5 w-3/5" />
                  <Skeleton className="hidden h-3.5 w-20 sm:block" />
                  <Skeleton className="h-3.5 w-16" />
                </div>
              ))
            : data?.map((t) => (
                <div key={t.id} className={cn(GRID, ROW_HEIGHT, "border-t border-border/60")}>
                  <span className="text-muted-foreground tabular-nums">{formatShortDate(t.txnDate)}</span>
                  <span className="truncate">{t.description ?? <span className="text-muted-foreground">—</span>}</span>
                  <span className="hidden min-w-0 text-muted-foreground sm:block">
                    <CategoryLabel transaction={t} category={t.categoryId ? categoryById.get(t.categoryId) : undefined} />
                  </span>
                  <AmountText type={t.type} amount={t.amount} className="text-right" />
                </div>
              ))}
        </div>
      )}
    </section>
  );
}
