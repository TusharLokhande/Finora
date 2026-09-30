import { useCallback, useState } from "react";
import { format } from "date-fns";
import { Plus } from "lucide-react";
import { Button } from "@/ui/button";
import { Skeleton } from "@/ui/skeleton";
import { useHotkey } from "@/hooks/useHotkey";
import { QuickAddSheet, QuickEntryForm, RecentTransactionsCard, useRecentTransactions } from "@/features/transactions";
import { useDashboardSummary } from "../hooks/queries/useDashboard";
import { KpiStrip, KpiStripSkeleton } from "../components/KpiStrip";
import { IncomeExpenseChart } from "../components/IncomeExpenseChart";
import { BudgetsAtRiskCard } from "../components/BudgetsAtRiskCard";
import { UpcomingDuesCard } from "../components/UpcomingDuesCard";
import { TopCategoriesCard } from "../components/TopCategoriesCard";
import { SectionError } from "@/components/SectionCard";

const RECENT_LIMIT = 8;

export function HomePage() {
  const [adding, setAdding] = useState(false);
  const summary = useDashboardSummary();
  // Shares its cache with the Recent transactions card; an empty list means the user has no transactions yet.
  const recent = useRecentTransactions(RECENT_LIMIT);
  const isEmpty = recent.data?.length === 0;

  useHotkey("n", useCallback(() => setAdding(true), []));

  return (
    <div className="flex w-full flex-col gap-6 p-4 md:p-6">
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="font-heading text-xl font-semibold text-foreground">Home</h1>
          <p className="text-sm text-muted-foreground">{format(new Date(), "EEEE, d MMMM")}</p>
        </div>
        {!isEmpty && (
          <Button onClick={() => setAdding(true)}>
            <Plus /> Add transaction
          </Button>
        )}
      </div>

      {isEmpty ? (
        <div className="mx-auto flex w-full max-w-md flex-col gap-4 py-10">
          <div className="space-y-1 text-center">
            <h2 className="font-heading text-lg font-semibold text-foreground">Log your first transaction</h2>
            <p className="text-sm text-muted-foreground">
              Your spending, income, budgets and card dues show up here once you start adding entries.
            </p>
          </div>
          <div className="rounded-xl border border-border bg-card p-4">
            <QuickEntryForm variant="stacked" />
          </div>
        </div>
      ) : (
        <>
          {summary.isError ? (
            <SectionError onRetry={summary.refetch} />
          ) : summary.data ? (
            <KpiStrip summary={summary.data} />
          ) : (
            <KpiStripSkeleton />
          )}

          <div className="grid gap-4 lg:grid-cols-3 lg:items-start">
            <div className="flex flex-col gap-4 lg:col-span-2">
              <IncomeExpenseChart />
              {recent.isLoading ? <Skeleton className="h-80 rounded-xl" /> : <RecentTransactionsCard limit={RECENT_LIMIT} />}
            </div>
            <div className="flex flex-col gap-4">
              <BudgetsAtRiskCard />
              <UpcomingDuesCard />
              <TopCategoriesCard />
            </div>
          </div>
        </>
      )}

      <QuickAddSheet open={adding} onOpenChange={setAdding} />
    </div>
  );
}
