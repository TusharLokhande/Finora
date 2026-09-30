import { useMemo } from "react";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { Skeleton } from "@/ui/skeleton";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/ui/dropdown-menu";
import { BUDGET_ROW } from "../constants/layout";
import type { BudgetLine, BudgetSort } from "../types/budget.types";
import { BudgetRow } from "./BudgetRow";

const SORT_LABELS: Record<BudgetSort, string> = {
  category: "Category order",
  mostUsed: "Most used first",
};

interface BudgetListProps {
  lines: BudgetLine[];
  /** A sub-category being budgeted for the first time, shown under its parent. */
  draft: BudgetLine | null;
  sort: BudgetSort;
  onSortChange: (sort: BudgetSort) => void;
  editingId: string | null;
  onEdit: (categoryId: string | null) => void;
  onSave: (line: BudgetLine, amount: number) => void;
}

interface Group {
  root: BudgetLine;
  children: BudgetLine[];
}

/** How "used" a line is for sorting: a zero budget with spend counts as fully used. */
const usage = (l: BudgetLine) => l.percentUsed ?? (l.status === "Over" ? Number.POSITIVE_INFINITY : 0);

/** Descending, and safe for Infinity (Infinity - Infinity would be NaN). */
const desc = (a: number, b: number) => (a === b ? 0 : a > b ? -1 : 1);

/** Parents with their budgeted sub-categories; any group without a budget always sinks to the bottom. */
function arrange(lines: BudgetLine[], draft: BudgetLine | null, sort: BudgetSort): Group[] {
  const groups: Group[] = lines
    .filter((l) => l.parentId === null)
    .map((root) => ({
      root,
      children: [
        ...lines.filter((l) => l.parentId === root.categoryId),
        ...(draft?.parentId === root.categoryId ? [draft] : []),
      ],
    }));

  const budgeted = (g: Group) => [g.root, ...g.children].some((l) => l.budgeted !== null);
  const score = (g: Group) => Math.max(...[g.root, ...g.children].filter((l) => l.budgeted !== null).map(usage));

  const withBudget = groups.filter(budgeted);
  const without = groups.filter((g) => !budgeted(g));

  if (sort === "mostUsed") {
    withBudget.sort((a, b) => desc(score(a), score(b)));
    for (const g of withBudget) g.children.sort((a, b) => desc(usage(a), usage(b)));
  }

  return [...withBudget, ...without];
}

export function BudgetList({ lines, draft, sort, onSortChange, editingId, onEdit, onSave }: BudgetListProps) {
  const groups = useMemo(() => arrange(lines, draft, sort), [lines, draft, sort]);

  const row = (line: BudgetLine) => (
    <BudgetRow
      key={line.categoryId}
      line={line}
      editing={editingId === line.categoryId}
      onStartEdit={() => onEdit(line.categoryId)}
      onCancel={() => onEdit(null)}
      onSave={(amount) => onSave(line, amount)}
    />
  );

  return (
    <div className="flex flex-col gap-2">
      <div className="flex justify-end">
        <DropdownMenu>
          <DropdownMenuTrigger className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground">
            Sort: {SORT_LABELS[sort].toLowerCase()} <ChevronDown className="size-3" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {(Object.keys(SORT_LABELS) as BudgetSort[]).map((key) => (
              <DropdownMenuItem key={key} onSelect={() => onSortChange(key)} className={cn(key === sort && "font-medium")}>
                {SORT_LABELS[key]}
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="divide-y divide-border rounded-xl border border-border bg-card">
        {groups.flatMap((g) => [row(g.root), ...g.children.map(row)])}
      </div>
    </div>
  );
}

export function BudgetListSkeleton() {
  return (
    <div className="divide-y divide-border rounded-xl border border-border bg-card" aria-hidden>
      {Array.from({ length: 5 }).map((_, i) => (
        <div key={i} className={BUDGET_ROW}>
          <div className="flex min-h-7 items-center gap-3">
            <Skeleton className="size-7 rounded-lg" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="ml-auto h-4 w-40" />
          </div>
          <Skeleton className="h-1.5 w-full" />
        </div>
      ))}
    </div>
  );
}
