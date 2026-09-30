import { formatCurrency } from "@/lib/currency";
import { Skeleton } from "@/ui/skeleton";
import { MetricTile } from "@/components/MetricTile";
import type { DashboardSummary } from "../types/dashboard.types";

type Tone = "good" | "bad" | "neutral";

const TONE: Record<Tone, string> = {
  good: "text-positive",
  bad: "text-negative",
  neutral: "text-muted-foreground",
};

/** "↓8% vs last month", toned by whether up is better. All numbers come from the backend. */
function comparison(percent: number | null, upIsGood: boolean): { text: string; tone: Tone } {
  if (percent === null) return { text: "Nothing last month", tone: "neutral" };
  if (percent === 0) return { text: "Same as last month", tone: "neutral" };
  const up = percent > 0;
  return {
    text: `${up ? "↑" : "↓"} ${Math.abs(percent)}% vs last month`,
    tone: up === upIsGood ? "good" : "bad",
  };
}

export function KpiStrip({ summary: s }: { summary: DashboardSummary }) {
  const spent = comparison(s.spentChangePercent, false);
  const income = comparison(s.incomeChangePercent, true);
  const net = comparison(s.netSavingsChangePercent, true);
  const { netSavings, budgeted, budgetRemaining } = s.current;

  return (
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
      <MetricTile label="Spent this month" value={formatCurrency(s.current.spent)}>
        <span className={TONE[spent.tone]}>{spent.text}</span>
      </MetricTile>

      <MetricTile label="Income this month" value={formatCurrency(s.current.income)}>
        <span className={TONE[income.tone]}>{income.text}</span>
      </MetricTile>

      <MetricTile
        label="Net savings"
        value={`${netSavings > 0 ? "+" : netSavings < 0 ? "−" : ""}${formatCurrency(Math.abs(netSavings))}`}
        valueClassName={netSavings > 0 ? "text-positive" : netSavings < 0 ? "text-negative" : undefined}
      >
        {s.savingsRatePercent !== null && <span className="text-muted-foreground">{s.savingsRatePercent}% of income · </span>}
        <span className={TONE[net.tone]}>{net.text}</span>
      </MetricTile>

      <MetricTile
        label="Budget remaining"
        value={`${budgetRemaining < 0 ? "−" : ""}${formatCurrency(Math.abs(budgetRemaining))}`}
        valueClassName={budgetRemaining < 0 ? "text-negative" : undefined}
      >
        <span className="text-muted-foreground">
          {budgeted > 0 ? `of ${formatCurrency(budgeted)} budgeted` : "No budgets set this month"}
        </span>
      </MetricTile>
    </div>
  );
}

export function KpiStripSkeleton() {
  return (
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4" aria-hidden>
      {Array.from({ length: 4 }).map((_, i) => (
        <div key={i} className="flex flex-col gap-2 px-1 py-2">
          <Skeleton className="h-4 w-28" />
          <Skeleton className="h-7 w-32" />
          <Skeleton className="h-3 w-24" />
        </div>
      ))}
    </div>
  );
}
