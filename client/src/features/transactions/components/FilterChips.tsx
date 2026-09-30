import type { Dispatch, SetStateAction } from "react";
import { format, parseISO } from "date-fns";
import { X } from "lucide-react";
import { formatCurrency, toMinorUnits } from "@/lib/currency";
import { DEFAULT_FILTERS, PERIOD_OPTIONS } from "../constants/filters";
import { useLookupOptions } from "../hooks/queries/useLookupOptions";
import type { TransactionFilters } from "../types/transaction.types";

interface FilterChipsProps {
  value: TransactionFilters;
  onChange: Dispatch<SetStateAction<TransactionFilters>>;
}

interface Chip {
  key: string;
  label: string;
  remove: (f: TransactionFilters) => TransactionFilters;
}

const money = (major: number) => formatCurrency(toMinorUnits(major));
const day = (iso: string | null) => (iso ? format(parseISO(iso), "d MMM yyyy") : "…");

/** One removable chip per active filter, plus "Clear all". Renders nothing when filters are default. */
export function FilterChips({ value: f, onChange }: FilterChipsProps) {
  const { accountById, categoryById } = useLookupOptions();
  const chips: Chip[] = [];

  if (f.period !== DEFAULT_FILTERS.period) {
    chips.push({
      key: "period",
      label:
        f.period === "custom"
          ? `${day(f.from)} – ${day(f.to)}`
          : (PERIOD_OPTIONS.find((o) => o.value === f.period)?.label ?? ""),
      remove: (x) => ({ ...x, period: DEFAULT_FILTERS.period, from: null, to: null }),
    });
  }

  for (const id of f.accountIds) {
    chips.push({
      key: `account-${id}`,
      label: accountById.get(id)?.label ?? "Account",
      remove: (x) => ({ ...x, accountIds: x.accountIds.filter((a) => a !== id) }),
    });
  }

  for (const id of f.categoryIds) {
    const category = categoryById.get(id);
    chips.push({
      key: `category-${id}`,
      label: category ? `${category.label}${category.parentId ? "" : " (all)"}` : "Category",
      remove: (x) => ({ ...x, categoryIds: x.categoryIds.filter((c) => c !== id) }),
    });
  }

  if (f.type) chips.push({ key: "type", label: f.type, remove: (x) => ({ ...x, type: null }) });

  if (f.minAmount != null || f.maxAmount != null) {
    chips.push({
      key: "amount",
      label:
        f.minAmount != null && f.maxAmount != null
          ? `${money(f.minAmount)} – ${money(f.maxAmount)}`
          : f.minAmount != null
            ? `≥ ${money(f.minAmount)}`
            : `≤ ${money(f.maxAmount!)}`,
      remove: (x) => ({ ...x, minAmount: null, maxAmount: null }),
    });
  }

  if (f.search.trim()) chips.push({ key: "search", label: `“${f.search.trim()}”`, remove: (x) => ({ ...x, search: "" }) });

  if (chips.length === 0) return null;

  return (
    <div className="flex flex-wrap items-center gap-1.5" aria-label="Active filters">
      {chips.map((chip) => (
        <span
          key={chip.key}
          className="inline-flex items-center gap-1 rounded-full bg-secondary py-0.5 pr-1 pl-2.5 text-xs text-secondary-foreground"
        >
          {chip.label}
          <button
            type="button"
            aria-label={`Remove filter ${chip.label}`}
            onClick={() => onChange(chip.remove)}
            className="rounded-full p-0.5 text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <X className="size-3" />
          </button>
        </span>
      ))}
      <button
        type="button"
        onClick={() => onChange(DEFAULT_FILTERS)}
        className="px-1.5 text-xs text-muted-foreground hover:text-foreground"
      >
        Clear all
      </button>
    </div>
  );
}
