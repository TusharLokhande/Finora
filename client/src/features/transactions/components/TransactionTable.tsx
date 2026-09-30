import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { Loader2, Tags, Trash2, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/ui/button";
import { Checkbox } from "@/ui/checkbox";
import { Select } from "@/ui/select";
import { Skeleton } from "@/ui/skeleton";
import { useLookupOptions, type LookupOption } from "../hooks/queries/useLookupOptions";
import { useUpdateTransaction } from "../hooks/mutations/useUpdateTransaction";
import { useDeleteTransactions } from "../hooks/mutations/useDeleteTransactions";
import { useBulkRecategorize } from "../hooks/mutations/useBulkRecategorize";
import { isTempId } from "../hooks/mutations/useOptimisticTransactionsMutation";
import type { CreateTransactionInput, Transaction } from "../types/transaction.types";
import { ROW_GRID, ROW_HEIGHT, formatDayLabel, netOf } from "../constants/ledger";
import { NetAmount } from "./TransactionCells";
import { TransactionRow } from "./TransactionRow";

interface TransactionTableProps {
  rows: Transaction[];
  totalCount: number;
  isLoading: boolean;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  onLoadMore: () => void;
  onEdit: (transaction: Transaction) => void;
  /** The pinned quick-add row, rendered between the header and the first day group. */
  quickAdd: ReactNode;
  /** Shown under the quick-add row when there are no rows. */
  emptyState: ReactNode;
}

export function TransactionTable({
  rows,
  totalCount,
  isLoading,
  hasNextPage,
  isFetchingNextPage,
  onLoadMore,
  onEdit,
  quickAdd,
  emptyState,
}: TransactionTableProps) {
  const lookups = useLookupOptions();
  const update = useUpdateTransaction();
  const deleteMutation = useDeleteTransactions();
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Drop selections that are no longer loaded (deleted, filtered away).
  const loadedIds = useMemo(() => new Set(rows.map((t) => t.id)), [rows]);
  const selected = rows.filter((t) => selectedIds.has(t.id));

  const toggle = useCallback(
    (id: string) =>
      setSelectedIds((prev) => {
        const next = new Set(prev);
        if (!next.delete(id)) next.add(id);
        return next;
      }),
    [],
  );
  const onUpdate = useCallback(
    (id: string, payload: CreateTransactionInput) => update.mutate({ id, payload }),
    // eslint-disable-next-line react-hooks/exhaustive-deps -- mutate is stable
    [],
  );
  const onDelete = useCallback(
    (t: Transaction) => deleteMutation.mutate([t]),
    // eslint-disable-next-line react-hooks/exhaustive-deps -- mutate is stable
    [],
  );

  const groups = useMemo(() => groupByDay(rows), [rows]);
  const selectable = rows.filter((t) => !isTempId(t.id));
  const allSelected = selectable.length > 0 && selectable.every((t) => selectedIds.has(t.id));

  return (
    <div role="table" aria-label="Transactions" className="overflow-clip rounded-xl border border-border bg-card">
      {selected.length > 0 ? (
        <BulkBar
          selected={selected}
          onClear={() => setSelectedIds(new Set())}
          onDeleted={() => setSelectedIds(new Set())}
        />
      ) : (
        <div
          role="row"
          className={cn(ROW_GRID, "h-9 border-b border-border text-xs font-medium text-muted-foreground")}
        >
          <Checkbox
            aria-label="Select all loaded"
            checked={allSelected}
            disabled={selectable.length === 0}
            onCheckedChange={() => setSelectedIds(allSelected ? new Set() : new Set(selectable.map((t) => t.id)))}
          />
          <span className="hidden md:block">Date</span>
          <span>Description</span>
          <span className="hidden md:block">Category</span>
          <span className="hidden md:block">Account</span>
          <span className="text-right">Amount</span>
          <span />
        </div>
      )}

      {quickAdd}

      {isLoading ? (
        <SkeletonRows />
      ) : rows.length === 0 ? (
        emptyState
      ) : (
        groups.map(({ date, rows: dayRows }) => (
          <div key={date} role="rowgroup">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-border/60 bg-muted/80 px-3 py-1.5 text-xs font-medium text-muted-foreground backdrop-blur">
              <span>{formatDayLabel(date)}</span>
              <NetAmount amount={netOf(dayRows)} />
            </div>
            {dayRows.map((t) => (
              <TransactionRow
                key={t.id}
                transaction={t}
                selected={selectedIds.has(t.id) && loadedIds.has(t.id)}
                selecting={selected.length > 0}
                onToggleSelect={toggle}
                onEdit={onEdit}
                onDelete={onDelete}
                onUpdate={onUpdate}
                lookups={lookups}
              />
            ))}
          </div>
        ))
      )}

      {!isLoading && rows.length > 0 && (
        <Footer
          shown={rows.length}
          total={totalCount}
          hasNextPage={hasNextPage}
          isFetchingNextPage={isFetchingNextPage}
          onLoadMore={onLoadMore}
        />
      )}
    </div>
  );
}

function groupByDay(rows: Transaction[]) {
  const groups: { date: string; rows: Transaction[] }[] = [];
  for (const t of rows) {
    const last = groups.at(-1);
    if (last?.date === t.txnDate) last.rows.push(t);
    else groups.push({ date: t.txnDate, rows: [t] });
  }
  return groups;
}

export function SkeletonRows({ count = 8 }: { count?: number }) {
  return (
    <div aria-hidden>
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className={cn(ROW_GRID, ROW_HEIGHT, "border-b border-border/60 last:border-b-0")}>
          <span />
          <Skeleton className="hidden h-3.5 w-12 md:block" />
          <Skeleton className="h-3.5 w-3/5" />
          <Skeleton className="hidden h-3.5 w-24 md:block" />
          <Skeleton className="hidden h-3.5 w-20 md:block" />
          <Skeleton className="h-3.5 w-20 justify-self-end" />
          <span />
        </div>
      ))}
    </div>
  );
}

/** Auto-loads the next page when scrolled into view; the button is the keyboard/no-IO fallback. */
function Footer({
  shown,
  total,
  hasNextPage,
  isFetchingNextPage,
  onLoadMore,
}: {
  shown: number;
  total: number;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  onLoadMore: () => void;
}) {
  const sentinel = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = sentinel.current;
    if (!el || !hasNextPage) return;
    const observer = new IntersectionObserver(([entry]) => entry.isIntersecting && !isFetchingNextPage && onLoadMore(), {
      rootMargin: "200px",
    });
    observer.observe(el);
    return () => observer.disconnect();
  }, [hasNextPage, isFetchingNextPage, onLoadMore]);

  return (
    <div ref={sentinel} className="flex items-center justify-between border-t border-border px-3 py-2 text-xs text-muted-foreground">
      <span className="tabular-nums">
        Showing {shown.toLocaleString()} of {total.toLocaleString()}
      </span>
      {hasNextPage && (
        <Button variant="ghost" size="sm" onClick={onLoadMore} disabled={isFetchingNextPage}>
          {isFetchingNextPage && <Loader2 className="animate-spin" />} Load more
        </Button>
      )}
    </div>
  );
}

function BulkBar({ selected, onClear, onDeleted }: { selected: Transaction[]; onClear: () => void; onDeleted: () => void }) {
  const { categoriesFor } = useLookupOptions();
  const deleteMutation = useDeleteTransactions();
  const recategorize = useBulkRecategorize();

  // Re-categorizing only makes sense when every selected row is the same non-transfer type.
  const types = new Set(selected.map((t) => t.type));
  const [onlyType] = types;
  const canRecategorize = types.size === 1 && onlyType !== "Transfer";

  function deleteSelected() {
    if (selected.length > 1 && !window.confirm(`Delete ${selected.length} transactions? You can undo right after.`)) return;
    deleteMutation.mutate(selected);
    onDeleted();
  }

  return (
    <div
      role="toolbar"
      aria-label="Bulk actions"
      className="flex h-9 items-center gap-2 border-b border-border bg-accent/40 px-3 text-sm"
    >
      <span className="font-medium tabular-nums">{selected.length} selected</span>

      <div className="ml-auto flex items-center gap-2">
        <Select<LookupOption>
          className="w-52"
          aria-label="Re-categorize selected"
          placeholder={canRecategorize ? "Re-categorize…" : "Re-categorize (one type only)"}
          isDisabled={!canRecategorize}
          options={canRecategorize ? categoriesFor(onlyType) : []}
          value={null}
          onChange={(o) => o && recategorize.mutate({ ids: selected.map((t) => t.id), categoryId: o.value })}
          menuPortalTarget={document.body}
          components={{ DropdownIndicator: () => <Tags className="mx-1 size-3.5 text-muted-foreground" /> }}
        />
        <Button variant="destructive" size="sm" onClick={deleteSelected}>
          <Trash2 /> Delete
        </Button>
        <Button variant="ghost" size="icon-sm" aria-label="Clear selection" onClick={onClear}>
          <X />
        </Button>
      </div>
    </div>
  );
}
