import { formatCurrency } from "@/lib/currency";

interface AccountSummaryStripProps {
  totalBalance: number;
  totalOwed: number;
  availableCredit: number;
}

export function AccountSummaryStrip({ totalBalance, totalOwed, availableCredit }: AccountSummaryStripProps) {
  const metrics = [
    { label: "Total balance", value: totalBalance },
    { label: "Total owed", value: totalOwed },
    { label: "Available credit", value: availableCredit },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      {metrics.map((metric) => (
        <div key={metric.label} className="rounded-xl bg-muted p-5">
          <p className="text-sm text-muted-foreground">{metric.label}</p>
          <p className="mt-2 font-mono text-2xl font-semibold tabular-nums text-foreground">
            {formatCurrency(metric.value)}
          </p>
        </div>
      ))}
    </div>
  );
}
