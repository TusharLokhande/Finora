import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { USAGE_TONE_TEXT, type UsageTone } from "@/lib/usageTone";
import { UsageBar } from "@/components/UsageBar";
import { useBudgetsAtRisk } from "../hooks/queries/useDashboard";
import { EmptyNote, ListSkeleton, SectionCard, SectionError } from "@/components/SectionCard";

/** Budgets at or over 80% this month; the backend filters, sorts and decides the state. */
export function BudgetsAtRiskCard() {
  const { data, isLoading, isError, refetch } = useBudgetsAtRisk();

  return (
    <SectionCard title="Budgets at risk">
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <ListSkeleton rows={2} />
      ) : data.length === 0 ? (
        <EmptyNote>All budgets are under 80%.</EmptyNote>
      ) : (
        <ul className="flex flex-col gap-3">
          {data.map((b) => {
            const tone = b.status.toLowerCase() as UsageTone;
            return (
              <li key={b.categoryId} className="flex flex-col gap-1.5">
                <div className="flex items-baseline justify-between gap-2 text-sm">
                  <span className="truncate" title={`${formatCurrency(b.spent)} of ${formatCurrency(b.budgeted ?? 0)}`}>
                    {b.parentCategoryName && <span className="text-muted-foreground">{b.parentCategoryName} › </span>}
                    {b.categoryName}
                  </span>
                  <span className={cn("shrink-0 font-mono text-xs tabular-nums", USAGE_TONE_TEXT[tone])}>
                    {b.status === "Over" && "Over · "}
                    {b.percentUsed != null ? `${Math.round(b.percentUsed)}%` : formatCurrency(b.spent)}
                  </span>
                </div>
                <UsageBar percent={b.percentUsed ?? 100} tone={tone} />
              </li>
            );
          })}
        </ul>
      )}
    </SectionCard>
  );
}
