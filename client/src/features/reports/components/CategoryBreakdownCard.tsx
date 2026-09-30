import { useState } from "react";
import { ChevronRight } from "lucide-react";
import { Cell, Pie, PieChart, Tooltip } from "recharts";
import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { Skeleton } from "@/ui/skeleton";
import { UsageBar } from "@/components/UsageBar";
import { ListSkeleton, SectionCard, SectionError } from "@/components/SectionCard";
import { useCategoryBreakdown, useSubcategoryBreakdown } from "../hooks/queries/useReports";
import type { CategorySpend, ReportRangeParams } from "../types/report.types";

// A segment's color follows its category (the user's own category color), never its rank, so a range
// change doesn't repaint survivors. Categories without a color fall back to the theme's chart tokens.
const FALLBACK = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)", "var(--chart-4)", "var(--chart-5)"];
const OTHER_COLOR = "var(--muted-foreground)";
const colorOf = (c: CategorySpend, index: number) => c.color ?? FALLBACK[index % FALLBACK.length];

interface Segment {
  key: string;
  name: string;
  spent: number;
  sharePercent: number;
  color: string;
}

export function CategoryBreakdownCard({ params }: { params: ReportRangeParams | null }) {
  const { data, isLoading, isError, refetch } = useCategoryBreakdown(params);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const segments: Segment[] = data
    ? [
        ...data.categories.map((c, i) => ({ key: c.categoryId, name: c.name, spent: c.spent, sharePercent: c.sharePercent, color: colorOf(c, i) })),
        ...(data.other
          ? [{ key: "other", name: `Other (${data.other.categoryCount} categories)`, spent: data.other.spent, sharePercent: data.other.sharePercent, color: OTHER_COLOR }]
          : []),
      ]
    : [];

  return (
    <SectionCard title="Where your money goes">
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <div className="grid items-center gap-6 md:grid-cols-[180px_minmax(0,1fr)]" aria-hidden>
          <Skeleton className="mx-auto size-44 rounded-full" />
          <ListSkeleton rows={5} />
        </div>
      ) : data.total === 0 ? (
        <p className="text-sm text-muted-foreground">No spending in this range.</p>
      ) : (
        <div className="grid items-center gap-6 md:grid-cols-[180px_minmax(0,1fr)]">
          <Donut segments={segments} total={data.total} />

          <ul className="flex flex-col">
            {data.categories.map((c, i) => (
              <CategoryRow
                key={c.categoryId}
                category={c}
                color={colorOf(c, i)}
                params={params}
                expanded={expandedId === c.categoryId}
                onToggle={() => setExpandedId((id) => (id === c.categoryId ? null : c.categoryId))}
              />
            ))}
            {data.other && (
              <li className="flex flex-col gap-1.5 py-2">
                <div className="flex items-center gap-2.5 text-sm">
                  <span className="size-2.5 shrink-0 rounded-full" style={{ backgroundColor: OTHER_COLOR }} />
                  <span className="min-w-0 flex-1 truncate text-muted-foreground">
                    Other ({data.other.categoryCount} {data.other.categoryCount === 1 ? "category" : "categories"})
                  </span>
                  <Amount spent={data.other.spent} share={data.other.sharePercent} />
                  <span className="size-4 shrink-0" />
                </div>
                <UsageBar percent={data.other.sharePercent} color={OTHER_COLOR} />
              </li>
            )}
          </ul>
        </div>
      )}
    </SectionCard>
  );
}

function Amount({ spent, share }: { spent: number; share: number }) {
  return (
    <>
      <span className="shrink-0 font-mono text-sm tabular-nums">{formatCurrency(spent)}</span>
      <span className="w-12 shrink-0 text-right font-mono text-xs tabular-nums text-muted-foreground">{Math.round(share)}%</span>
    </>
  );
}

interface CategoryRowProps {
  category: CategorySpend;
  color: string;
  params: ReportRangeParams | null;
  expanded: boolean;
  onToggle: () => void;
}

/** A ranked category; clicking one with sub-categories expands them in place (shares of the parent). */
function CategoryRow({ category: c, color, params, expanded, onToggle }: CategoryRowProps) {
  const subs = useSubcategoryBreakdown(c.categoryId, params, expanded);
  const header = (
    <>
      <span className="size-2.5 shrink-0 rounded-full" style={{ backgroundColor: color }} />
      <span className="min-w-0 flex-1 truncate text-left">{c.name}</span>
      <Amount spent={c.spent} share={c.sharePercent} />
      {c.hasSubcategories ? (
        <ChevronRight className={cn("size-4 shrink-0 text-muted-foreground transition-transform", expanded && "rotate-90")} />
      ) : (
        <span className="size-4 shrink-0" />
      )}
    </>
  );

  return (
    <li className="flex flex-col gap-1.5 py-2">
      {c.hasSubcategories ? (
        <button
          type="button"
          aria-expanded={expanded}
          onClick={onToggle}
          className="-mx-1.5 flex items-center gap-2.5 rounded-md px-1.5 py-0.5 text-sm hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring/50 focus-visible:outline-none"
        >
          {header}
        </button>
      ) : (
        <div className="flex items-center gap-2.5 text-sm">{header}</div>
      )}
      <UsageBar percent={c.sharePercent} color={color} />

      {expanded && (
        <div className="mt-1 flex flex-col gap-2 border-l border-border pl-4">
          {subs.isError ? (
            <SectionError onRetry={subs.refetch} />
          ) : subs.isLoading || !subs.data ? (
            <ListSkeleton rows={2} />
          ) : (
            subs.data.categories.map((s) => (
              <div key={s.categoryId} className="flex flex-col gap-1">
                <div className="flex items-center gap-2.5 text-xs">
                  <span className="min-w-0 flex-1 truncate text-muted-foreground">{s.name}</span>
                  <span className="shrink-0 font-mono tabular-nums">{formatCurrency(s.spent)}</span>
                  <span className="w-12 shrink-0 text-right font-mono tabular-nums text-muted-foreground">
                    {Math.round(s.sharePercent)}%
                  </span>
                  <span className="size-4 shrink-0" />
                </div>
                <UsageBar percent={s.sharePercent} color={color} className="h-1 opacity-70" />
              </div>
            ))
          )}
        </div>
      )}
    </li>
  );
}

function Donut({ segments, total }: { segments: Segment[]; total: number }) {
  return (
    <div className="relative mx-auto size-44">
      <PieChart width={176} height={176}>
        <Pie
          data={segments}
          dataKey="spent"
          nameKey="name"
          innerRadius={58}
          outerRadius={84}
          paddingAngle={segments.length > 1 ? 2 : 0}
          stroke="var(--card)"
          strokeWidth={segments.length > 1 ? 2 : 0}
          isAnimationActive={false}
        >
          {segments.map((s) => (
            <Cell key={s.key} fill={s.color} />
          ))}
        </Pie>
        <Tooltip
          content={({ active, payload }) => {
            const s = active ? (payload?.[0]?.payload as Segment | undefined) : undefined;
            return s ? (
              <div className="rounded-lg border border-border bg-popover px-2.5 py-1.5 text-xs shadow-md">
                <p className="font-medium text-foreground">{s.name}</p>
                <p className="font-mono tabular-nums text-muted-foreground">
                  {formatCurrency(s.spent)} · {s.sharePercent}%
                </p>
              </div>
            ) : null;
          }}
        />
      </PieChart>
      <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
        <span className="font-heading text-sm font-semibold tabular-nums text-foreground">{formatCurrency(total)}</span>
        <span className="text-[11px] text-muted-foreground">total spent</span>
      </div>
      {/* The ranked list beside the donut is its text alternative. */}
    </div>
  );
}
