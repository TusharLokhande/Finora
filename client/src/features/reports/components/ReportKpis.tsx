import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { Skeleton } from "@/ui/skeleton";
import { MetricTile } from "@/components/MetricTile";
import type { ReportSummary } from "../types/report.types";

/** "↓ 6%" toned by whether down is good; nothing when there is no prior value to compare with. */
function Change({ percent, upIsGood }: { percent: number | null; upIsGood: boolean }) {
  if (percent === null || percent === 0) return null;
  const up = percent > 0;
  return (
    <span className={cn(up === upIsGood ? "text-positive" : "text-negative")}>
      {up ? "↑" : "↓"} {Math.abs(percent)}%{" "}
    </span>
  );
}

const signed = (minor: number) => `${minor > 0 ? "+" : minor < 0 ? "−" : ""}${formatCurrency(Math.abs(minor))}`;

/** Total spent / income / net savings, each against the equally long period before it (all from the backend). */
export function ReportKpis({ summary: s }: { summary: ReportSummary }) {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <MetricTile label="Total spent" value={formatCurrency(s.spent)}>
        <Change percent={s.spentChangePercent} upIsGood={false} />
        <span className="text-muted-foreground">vs {formatCurrency(s.priorSpent)} prior period</span>
      </MetricTile>

      <MetricTile label="Total income" value={formatCurrency(s.income)}>
        <Change percent={s.incomeChangePercent} upIsGood />
        <span className="text-muted-foreground">vs {formatCurrency(s.priorIncome)} prior period</span>
      </MetricTile>

      <MetricTile
        label="Net savings"
        value={signed(s.netSavings)}
        valueClassName={s.netSavings > 0 ? "text-positive" : s.netSavings < 0 ? "text-negative" : undefined}
      >
        {s.savingsRatePercent !== null && <span className="text-muted-foreground">{s.savingsRatePercent}% of income · </span>}
        <span className="text-muted-foreground">vs {signed(s.priorNetSavings)} prior</span>
      </MetricTile>
    </div>
  );
}

export function ReportKpisSkeleton() {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3" aria-hidden>
      {Array.from({ length: 3 }).map((_, i) => (
        <div key={i} className="flex flex-col gap-2 px-1 py-2">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-7 w-32" />
          <Skeleton className="h-3 w-40" />
        </div>
      ))}
    </div>
  );
}
