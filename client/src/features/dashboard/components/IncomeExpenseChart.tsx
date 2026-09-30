import { format, parseISO } from "date-fns";
import { Bar, BarChart, CartesianGrid, XAxis } from "recharts";
import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { Skeleton } from "@/ui/skeleton";
import {
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/ui/chart";
import { SectionCard, SectionError } from "@/components/SectionCard";
import { useIncomeExpense } from "../hooks/queries/useDashboard";
import type { MonthlyTotal } from "../types/dashboard.types";

// Validated with the dataviz palette checks (lightness band, chroma, CVD, contrast) on both surfaces.
const chartConfig = {
  income: { label: "Income", theme: { light: "oklch(0.5 0.11 150)", dark: "oklch(0.64 0.12 150)" } },
  expense: { label: "Expense", theme: { light: "oklch(0.6 0.14 255)", dark: "oklch(0.58 0.14 260)" } },
} satisfies ChartConfig;

/** Grouped income/expense bars, one pair per month, with legend, hover tooltip and a screen-reader table. */
export function IncomeExpenseBars({ data, className }: { data: MonthlyTotal[]; className?: string }) {
  // Past a year, "Sep" alone is ambiguous.
  const tickFormat = data.length > 12 ? "MMM yy" : "MMM";

  return (
    <>
      <ChartContainer config={chartConfig} className={cn("aspect-auto h-44 w-full", className)}>
        <BarChart data={data} barGap={2} margin={{ top: 4, right: 0, left: 0, bottom: 0 }}>
          <CartesianGrid vertical={false} />
          <XAxis
            dataKey="month"
            tickFormatter={(iso: string) => format(parseISO(iso), tickFormat)}
            tickLine={false}
            axisLine={false}
            tickMargin={8}
          />
          <ChartTooltip
            cursor={{ fillOpacity: 0.4 }}
            content={
              <ChartTooltipContent
                labelFormatter={(_, payload) => format(parseISO(payload[0]?.payload.month), "MMMM yyyy")}
                formatter={(value, name) => (
                  <div className="flex w-full justify-between gap-4">
                    <span className="text-muted-foreground">{chartConfig[name as keyof typeof chartConfig].label}</span>
                    <span className="font-mono tabular-nums text-foreground">{formatCurrency(Number(value))}</span>
                  </div>
                )}
              />
            }
          />
          <ChartLegend content={<ChartLegendContent />} />
          <Bar dataKey="income" fill="var(--color-income)" radius={[4, 4, 0, 0]} maxBarSize={18} />
          <Bar dataKey="expense" fill="var(--color-expense)" radius={[4, 4, 0, 0]} maxBarSize={18} />
        </BarChart>
      </ChartContainer>

      {/* Text alternative for screen readers. */}
      <table className="sr-only">
        <caption>Income and expense by month</caption>
        <thead>
          <tr>
            <th>Month</th>
            <th>Income</th>
            <th>Expense</th>
          </tr>
        </thead>
        <tbody>
          {data.map((m) => (
            <tr key={m.month}>
              <td>{format(parseISO(m.month), "MMMM yyyy")}</td>
              <td>{formatCurrency(m.income)}</td>
              <td>{formatCurrency(m.expense)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}

/** Home's widget: the last 6 months. */
export function IncomeExpenseChart() {
  const { data, isLoading, isError, refetch } = useIncomeExpense(6);

  return (
    <SectionCard title="Income vs expense">
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <Skeleton className="h-44 w-full" />
      ) : (
        <IncomeExpenseBars data={data} />
      )}
    </SectionCard>
  );
}
