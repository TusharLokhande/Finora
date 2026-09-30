import { Link } from "react-router-dom";
import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { USAGE_TONE_SOFT, USAGE_TONE_TEXT, toneFor } from "@/lib/usageTone";
import { UsageBar } from "@/components/UsageBar";
import { EmptyNote, ListSkeleton, SectionCard, SectionError } from "@/components/SectionCard";
import { getCategoryIcon } from "@/features/categories";
import { useBudgetVsActual } from "../hooks/queries/useReports";
import type { ReportRangeParams } from "../types/report.types";

interface BudgetVsActualCardProps {
  params: ReportRangeParams | null;
  /** e.g. "the last 6 months" */
  rangeLabel: string;
}

/** Budget and spend summed over the budgeted months in range; same state colors as the Budgets page. */
export function BudgetVsActualCard({ params, rangeLabel }: BudgetVsActualCardProps) {
  const { data, isLoading, isError, refetch } = useBudgetVsActual(params);

  return (
    <SectionCard
      title="Budget vs actual"
      subtitle={`Summed across ${rangeLabel}.`}
    >
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <ListSkeleton rows={4} />
      ) : data.lines.length === 0 ? (
        <EmptyNote>
          No budgets in this range.{" "}
          <Link to="/budgets" className="text-primary hover:underline">
            Set budgets
          </Link>
        </EmptyNote>
      ) : (
        <ul className="-my-1 divide-y divide-border/60">
          {data.lines.map((line) => {
            const tone = toneFor(line.status);
            const Icon = getCategoryIcon(line.icon);
            return (
              <li key={line.categoryId} className="flex flex-col gap-2 py-2.5">
                <div className="flex items-center gap-2.5 text-sm">
                  <span className={cn("flex size-6 shrink-0 items-center justify-center rounded-md", USAGE_TONE_SOFT[tone])}>
                    <Icon className="size-3.5" />
                  </span>
                  <span className="min-w-0 flex-1 truncate">
                    {line.parentCategoryName && <span className="text-muted-foreground">{line.parentCategoryName} › </span>}
                    {line.categoryName}
                  </span>
                  <span className={cn("shrink-0 font-mono text-xs tabular-nums", line.status === "Normal" ? "text-foreground" : USAGE_TONE_TEXT[tone])}>
                    {formatCurrency(line.spent)} of {formatCurrency(line.budgeted)}
                    {line.percentUsed !== null && ` (${Math.round(line.percentUsed)}%)`}
                  </span>
                </div>
                <UsageBar percent={line.percentUsed ?? 100} tone={tone} />
              </li>
            );
          })}
        </ul>
      )}
    </SectionCard>
  );
}
