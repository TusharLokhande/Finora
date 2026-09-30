import { format, isToday, isYesterday, parseISO } from "date-fns";
import type { Transaction } from "../types/transaction.types";

/**
 * Column template shared by the header, quick-add-aligned rows, skeletons and the dashboard card:
 * select · date · description · category · account · amount · actions. Mobile keeps
 * select · description · amount · actions.
 */
export const ROW_GRID =
  "grid items-center gap-x-3 px-3 grid-cols-[1rem_minmax(0,1fr)_auto_1.75rem] md:grid-cols-[1rem_4.5rem_minmax(0,1fr)_minmax(0,11rem)_minmax(0,9rem)_8.5rem_1.75rem]";

/** Real row height; skeletons use the same value so nothing jumps when data lands. */
export const ROW_HEIGHT = "h-11";

export function formatDayLabel(isoDate: string): string {
  const date = parseISO(isoDate);
  if (isToday(date)) return "Today";
  if (isYesterday(date)) return "Yesterday";
  return format(date, "EEE, d MMM yyyy");
}

export function formatShortDate(isoDate: string): string {
  return format(parseISO(isoDate), "d MMM");
}

export function netOf(rows: Transaction[]): number {
  return rows.reduce((sum, t) => sum + (t.type === "Income" ? t.amount : t.type === "Expense" ? -t.amount : 0), 0);
}
