import { useState } from "react";
import { Link } from "react-router-dom";
import { BarChart3, Download } from "lucide-react";
import { Button } from "@/ui/button";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { PeriodRangePicker, type PeriodRange } from "@/components/PeriodRangePicker";
import { SectionError } from "@/components/SectionCard";
import { DEFAULT_RANGE, describeRange, toParams } from "../constants/range";
import { useReportSummary } from "../hooks/queries/useReports";
import { useExportReport } from "../hooks/mutations/useExportReport";
import { ReportKpis, ReportKpisSkeleton } from "../components/ReportKpis";
import { CategoryBreakdownCard } from "../components/CategoryBreakdownCard";
import { IncomeExpenseCard } from "../components/IncomeExpenseCard";
import { BudgetVsActualCard } from "../components/BudgetVsActualCard";

export function ReportsPage() {
  const [range, setRange] = useState<PeriodRange>(DEFAULT_RANGE);
  const params = toParams(range);
  const summary = useReportSummary(params);
  const exportMutation = useExportReport();

  // Nothing logged in range (transfers don't count): one message instead of four empty cards.
  const isEmpty = summary.data?.spent === 0 && summary.data.income === 0;

  return (
    <div className="flex w-full flex-col gap-5 p-4 md:p-6">
      <PageBreadcrumb items={["Reports"]} />
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="font-heading text-xl font-semibold text-foreground">Reports</h1>
          <p className="text-sm text-muted-foreground">See where your money goes.</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <PeriodRangePicker value={range} onChange={(patch) => setRange((r) => ({ ...r, ...patch }))} />
          <Button
            variant="outline"
            disabled={!params || exportMutation.isPending}
            onClick={() => params && exportMutation.mutate(params)}
          >
            <Download /> Export report
          </Button>
        </div>
      </div>

      {!params && <p className="text-sm text-muted-foreground">Pick a start and end date to see this range.</p>}

      {summary.isError ? (
        <SectionError onRetry={summary.refetch} />
      ) : summary.data ? (
        <ReportKpis summary={summary.data} />
      ) : (
        <ReportKpisSkeleton />
      )}

      {isEmpty ? (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-border px-3 py-16 text-center">
          <BarChart3 className="size-8 text-muted-foreground/50" />
          <p className="text-sm text-muted-foreground">
            No income or spending in {describeRange(range)}. Try a longer range, or add some transactions.
          </p>
          <Button asChild variant="outline">
            <Link to="/transactions">Go to Transactions</Link>
          </Button>
        </div>
      ) : (
        <>
          <CategoryBreakdownCard params={params} />
          <IncomeExpenseCard params={params} />
          <BudgetVsActualCard params={params} rangeLabel={describeRange(range)} />
        </>
      )}
    </div>
  );
}
