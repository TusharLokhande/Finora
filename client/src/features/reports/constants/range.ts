import { format, parseISO } from "date-fns";
import type { PeriodRange } from "@/components/PeriodRangePicker";
import type { ReportRangeParams } from "../types/report.types";

export const DEFAULT_RANGE: PeriodRange = { period: "6", from: null, to: null };

/** Query params for the API; null while a custom range is half-filled (queries wait). */
export function toParams(range: PeriodRange): ReportRangeParams | null {
  if (range.period !== "custom") return { months: Number(range.period) };
  return range.from && range.to ? { from: range.from, to: range.to } : null;
}

/** "the last 6 months", "this month", "1 Jan 2026 – 31 Mar 2026". */
export function describeRange(range: PeriodRange): string {
  if (range.period === "1") return "this month";
  if (range.period !== "custom") return `the last ${range.period} months`;
  const d = (iso: string | null) => (iso ? format(parseISO(iso), "d MMM yyyy") : "…");
  return `${d(range.from)} – ${d(range.to)}`;
}
