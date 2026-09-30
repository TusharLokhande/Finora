import { Skeleton } from "@/ui/skeleton";
import { SectionCard, SectionError } from "@/components/SectionCard";
import { IncomeExpenseBars } from "@/features/dashboard";
import { useReportIncomeVsExpense } from "../hooks/queries/useReports";
import type { ReportRangeParams } from "../types/report.types";

/** The Home dashboard's chart over the selected range: the trend view under the proportion view. */
export function IncomeExpenseCard({ params }: { params: ReportRangeParams | null }) {
  const { data, isLoading, isError, refetch } = useReportIncomeVsExpense(params);

  return (
    <SectionCard title="Income vs expense">
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <Skeleton className="h-56 w-full" />
      ) : (
        <IncomeExpenseBars data={data} className="h-56" />
      )}
    </SectionCard>
  );
}
