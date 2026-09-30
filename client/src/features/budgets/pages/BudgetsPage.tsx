import { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { RefreshCw, Tags } from "lucide-react";
import { Button } from "@/ui/button";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { monthFromParam, monthToParam, shiftMonth } from "../constants/months";
import { useBudgetMonth, useHasBudgets } from "../hooks/queries/useBudgets";
import { useUnbudgetedCategories } from "../hooks/queries/useUnbudgetedCategories";
import { useSetBudget } from "../hooks/mutations/useSetBudget";
import { useCopyBudgets } from "../hooks/mutations/useCopyBudgets";
import { MonthSwitcher } from "../components/MonthSwitcher";
import { NoBudgetsBanner } from "../components/NoBudgetsBanner";
import { BudgetSummarySkeleton, BudgetSummaryStrip } from "../components/BudgetSummaryStrip";
import { BudgetList, BudgetListSkeleton } from "../components/BudgetList";
import { AddBudgetButton } from "../components/AddBudgetButton";
import type { BudgetSort } from "../types/budget.types";

export function BudgetsPage() {
  // The month lives in the URL (?month=2026-09) so it survives reloads and can be linked to.
  const [searchParams, setSearchParams] = useSearchParams();
  const month = monthFromParam(searchParams.get("month"));
  const setMonth = (next: string) => {
    setEditingId(null);
    setSearchParams({ month: monthToParam(next) }, { replace: true });
  };

  const { data, isLoading, isError, refetch, isPlaceholderData } = useBudgetMonth(month);
  const hasAny = useHasBudgets(month);
  const previousHasAny = useHasBudgets(shiftMonth(month, -1));
  const setBudget = useSetBudget();
  const copy = useCopyBudgets();

  const [sort, setSort] = useState<BudgetSort>("category");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [dismissed, setDismissed] = useState<Set<string>>(new Set());

  const lines = data?.lines ?? [];
  const unbudgeted = useUnbudgetedCategories(lines);
  // A sub-category being budgeted for the first time has no row yet; show a stub under its parent.
  const draftOption = unbudgeted.find((o) => o.value === editingId && !lines.some((l) => l.categoryId === o.value));
  const draft = draftOption?.stub ?? null;

  const showBanner = hasAny.data === false && !dismissed.has(month);
  const noCategories = !!data && lines.length === 0;

  return (
    <div className="flex w-full flex-col gap-5 p-4 md:p-6">
      <PageBreadcrumb items={["Budgets"]} />
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="font-heading text-xl font-semibold text-foreground">Budgets</h1>
          <p className="text-sm text-muted-foreground">Monthly limits per category.</p>
        </div>
        <AddBudgetButton options={unbudgeted} onPick={(o) => setEditingId(o.value)} />
      </div>

      <MonthSwitcher month={month} onChange={setMonth} />

      {isError ? (
        <div role="alert" className="flex items-center justify-between gap-3 rounded-lg border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive">
          Couldn&apos;t load budgets.
          <Button variant="outline" size="sm" onClick={() => refetch()}>
            <RefreshCw /> Retry
          </Button>
        </div>
      ) : noCategories ? (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-border px-3 py-16 text-center">
          <Tags className="size-8 text-muted-foreground/50" />
          <p className="text-sm text-muted-foreground">You don&apos;t have any expense categories to budget yet.</p>
          <Button asChild variant="outline">
            <Link to="/categories">Go to Categories</Link>
          </Button>
        </div>
      ) : (
        <>
          {showBanner && (
            <NoBudgetsBanner
              month={month}
              canCopy={previousHasAny.data === true}
              copying={copy.isPending}
              onCopy={() => copy.mutate(month)}
              onStartFresh={() => setDismissed((d) => new Set(d).add(month))}
            />
          )}

          {isLoading || !data ? <BudgetSummarySkeleton /> : <BudgetSummaryStrip month={data} />}

          {isLoading || !data ? (
            <BudgetListSkeleton />
          ) : (
            <div className={isPlaceholderData ? "opacity-60 transition-opacity" : undefined}>
              <BudgetList
                lines={lines}
                draft={draft}
                sort={sort}
                onSortChange={setSort}
                editingId={editingId}
                onEdit={setEditingId}
                onSave={(line, amount) => {
                  setEditingId(null);
                  setBudget.mutate({ month, line, amount });
                }}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
}
