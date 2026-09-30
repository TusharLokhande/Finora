import { formatCurrency } from "@/lib/currency";
import { getCategoryIcon } from "@/features/categories";
import { UsageBar } from "@/components/UsageBar";
import { useTopCategories } from "../hooks/queries/useDashboard";
import { EmptyNote, ListSkeleton, SectionCard, SectionError } from "@/components/SectionCard";

/** This month's top expense categories (sub-categories rolled up), each with its share of total spend. */
export function TopCategoriesCard() {
  const { data, isLoading, isError, refetch } = useTopCategories(3);

  return (
    <SectionCard title="Top categories">
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <ListSkeleton rows={3} />
      ) : data.length === 0 ? (
        <EmptyNote>No spending yet this month.</EmptyNote>
      ) : (
        <ol className="flex flex-col gap-3">
          {data.map((c) => {
            const Icon = getCategoryIcon(c.icon);
            return (
              <li key={c.categoryId} className="flex flex-col gap-1.5">
                <div className="flex items-center justify-between gap-2 text-sm">
                  <span className="flex min-w-0 items-center gap-2">
                    <Icon className="size-3.5 shrink-0 text-muted-foreground" />
                    <span className="truncate">{c.name}</span>
                  </span>
                  <span className="shrink-0 font-mono text-xs tabular-nums text-muted-foreground">
                    {formatCurrency(c.spent)} · {c.sharePercent}%
                  </span>
                </div>
                <UsageBar percent={c.sharePercent} />
              </li>
            );
          })}
        </ol>
      )}
    </SectionCard>
  );
}
