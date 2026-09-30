import { useCallback, useEffect, useRef, useState, type Dispatch, type SetStateAction } from "react";
import { Search, SlidersHorizontal, X } from "lucide-react";
import type { MultiValue } from "react-select";
import { cn } from "@/lib/utils";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { Label } from "@/ui/label";
import { Select } from "@/ui/select";
import { Popover, PopoverContent, PopoverTrigger } from "@/ui/popover";
import { PeriodRangePicker } from "@/components/PeriodRangePicker";
import { useHotkey } from "@/hooks/useHotkey";
import { TRANSACTION_TYPES } from "../constants/filters";
import { useLookupOptions, type CategoryOption, type LookupOption } from "../hooks/queries/useLookupOptions";
import type { TransactionFilters, TransactionType } from "../types/transaction.types";

interface TransactionFiltersBarProps {
  value: TransactionFilters;
  onChange: Dispatch<SetStateAction<TransactionFilters>>;
}

const typeOptions: LookupOption[] = [
  { value: "", label: "All types" },
  ...TRANSACTION_TYPES.map((t) => ({ value: t, label: t })),
];

// Menus portal out so the no-wrap row can scroll horizontally on small screens without clipping them.
const portal = { menuPortalTarget: typeof document !== "undefined" ? document.body : null, menuPosition: "fixed" } as const;

/** Multi-select that shows "Label · n" instead of growing the row; selections appear as chips below. */
const multiProps = {
  isMulti: true,
  controlShouldRenderValue: false,
  hideSelectedOptions: false,
  closeMenuOnSelect: false,
  isClearable: false,
  ...portal,
} as const;

export function TransactionFiltersBar({ value, onChange }: TransactionFiltersBarProps) {
  const { allAccounts, allCategories } = useLookupOptions();
  const [search, setSearch] = useState(value.search);
  const searchRef = useRef<HTMLInputElement>(null);

  useHotkey("/", useCallback(() => searchRef.current?.focus(), []));

  // Chips can clear the search from outside; keep the input in step (adjust-during-render, no effect).
  const [syncedSearch, setSyncedSearch] = useState(value.search);
  if (value.search !== syncedSearch) {
    setSyncedSearch(value.search);
    setSearch(value.search);
  }

  useEffect(() => {
    const id = setTimeout(() => onChange((f) => (f.search === search ? f : { ...f, search })), 300);
    return () => clearTimeout(id);
  }, [search, onChange]);

  const set = (patch: Partial<TransactionFilters>) => onChange((f) => ({ ...f, ...patch }));
  const ids = (options: MultiValue<LookupOption>) => options.map((o) => o.value);
  const countLabel = (label: string, n: number) => (n ? `${label} · ${n}` : label);

  return (
    <div className="-mx-1 flex flex-nowrap items-center gap-2 overflow-x-auto px-1 py-1">
      <PeriodRangePicker value={value} onChange={set} />

      <Select<LookupOption, true>
        {...multiProps}
        className="w-36 shrink-0"
        aria-label="Accounts"
        placeholder={countLabel("Accounts", value.accountIds.length)}
        options={allAccounts}
        value={allAccounts.filter((a) => value.accountIds.includes(a.value))}
        onChange={(o) => set({ accountIds: ids(o) })}
      />

      <Select<CategoryOption, true>
        {...multiProps}
        className="w-40 shrink-0"
        aria-label="Categories"
        placeholder={countLabel("Categories", value.categoryIds.length)}
        options={allCategories}
        value={allCategories.filter((c) => value.categoryIds.includes(c.value))}
        onChange={(o) => set({ categoryIds: ids(o) })}
        formatOptionLabel={(c, { context }) =>
          context === "menu" && c.parentId ? <span className="pl-4 text-muted-foreground">{c.name}</span> : c.label
        }
      />

      <Select<LookupOption>
        className="w-32 shrink-0"
        aria-label="Type"
        isSearchable={false}
        options={typeOptions}
        value={typeOptions.find((o) => o.value === (value.type ?? "")) ?? null}
        onChange={(o) => set({ type: (o?.value || null) as TransactionType | null })}
        {...portal}
      />

      <AmountFilter
        min={value.minAmount}
        max={value.maxAmount}
        onApply={(minAmount, maxAmount) => set({ minAmount, maxAmount })}
      />

      <div className="relative ml-auto min-w-44 flex-1">
        <Search className="pointer-events-none absolute top-1/2 left-2.5 size-3.5 -translate-y-1/2 text-muted-foreground" />
        <Input
          ref={searchRef}
          className="pl-8"
          aria-label="Search description and notes"
          placeholder="Search description or notes   /"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>
    </div>
  );
}

interface AmountFilterProps {
  min: number | null;
  max: number | null;
  onApply: (min: number | null, max: number | null) => void;
}

function AmountFilter({ min, max, onApply }: AmountFilterProps) {
  const [open, setOpen] = useState(false);
  const [draftMin, setDraftMin] = useState("");
  const [draftMax, setDraftMax] = useState("");
  const isActive = min != null || max != null;

  function handleOpenChange(next: boolean) {
    if (next) {
      setDraftMin(min?.toString() ?? "");
      setDraftMax(max?.toString() ?? "");
    }
    setOpen(next);
  }

  function apply(nextMin: string, nextMax: string) {
    onApply(nextMin === "" ? null : Number(nextMin), nextMax === "" ? null : Number(nextMax));
    setOpen(false);
  }

  return (
    <Popover open={open} onOpenChange={handleOpenChange}>
      <PopoverTrigger asChild>
        <Button variant="outline" className={cn("shrink-0", isActive && "border-primary text-primary")}>
          <SlidersHorizontal /> Amount
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-60">
        <form
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            apply(draftMin, draftMax);
          }}
        >
          <div className="grid grid-cols-2 gap-2">
            <Label className="flex flex-col items-start gap-1 text-xs text-muted-foreground">
              Min
              <Input type="number" min={0} step="0.01" value={draftMin} onChange={(e) => setDraftMin(e.target.value)} />
            </Label>
            <Label className="flex flex-col items-start gap-1 text-xs text-muted-foreground">
              Max
              <Input type="number" min={0} step="0.01" value={draftMax} onChange={(e) => setDraftMax(e.target.value)} />
            </Label>
          </div>
          <div className="flex justify-between gap-2">
            <Button type="button" variant="ghost" size="sm" disabled={!isActive} onClick={() => apply("", "")}>
              <X /> Clear
            </Button>
            <Button type="submit" size="sm">
              Apply
            </Button>
          </div>
        </form>
      </PopoverContent>
    </Popover>
  );
}
