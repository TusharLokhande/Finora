import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { Skeleton } from "@/ui/skeleton";
import type { BudgetMonth } from "../types/budget.types";

/** Plain totals — deliberately no overall bar, since there is no overall monthly cap. Styled like the Accounts strip. */
export function BudgetSummaryStrip({ month }: { month: BudgetMonth }) {
  const metrics = [
    { label: "Total budgeted", value: formatCurrency(month.totalBudgeted) },
    { label: "Total spent", value: formatCurrency(month.totalSpent) },
    {
      label: month.remaining < 0 ? "Over budget" : "Remaining",
      value: formatCurrency(Math.abs(month.remaining)),
      className: month.remaining < 0 ? "text-negative" : undefined,
    },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      {metrics.map((m) => (
        <div key={m.label} className="rounded-xl bg-muted p-5">
          <p className="text-sm text-muted-foreground">{m.label}</p>
          <p className={cn("mt-2 font-mono text-2xl font-semibold tabular-nums text-foreground", m.className)}>{m.value}</p>
        </div>
      ))}
    </div>
  );
}

export function BudgetSummarySkeleton() {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3" aria-hidden>
      {Array.from({ length: 3 }).map((_, i) => (
        <div key={i} className="rounded-xl bg-muted p-5">
          <Skeleton className="h-4 w-24 bg-background/60" />
          <Skeleton className="mt-3 h-7 w-32 bg-background/60" />
        </div>
      ))}
    </div>
  );
}
