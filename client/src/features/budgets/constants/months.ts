import { addMonths, format, parse, startOfMonth } from "date-fns";

/** Months are passed around as the backend's "yyyy-MM-01". */
export const MONTH_FORMAT = "yyyy-MM-01";

export const currentMonth = () => format(startOfMonth(new Date()), MONTH_FORMAT);

export const parseMonth = (month: string) => parse(month, "yyyy-MM-dd", new Date());

export const shiftMonth = (month: string, by: number) => format(addMonths(parseMonth(month), by), MONTH_FORMAT);

/** "September 2026" */
export const monthLabel = (month: string) => format(parseMonth(month), "MMMM yyyy");

/** "September" */
export const monthName = (month: string) => format(parseMonth(month), "MMMM");

/** URL form "2026-09" ⇄ "2026-09-01"; anything unparsable falls back to the current month. */
export function monthFromParam(param: string | null): string {
  return param && /^\d{4}-\d{2}$/.test(param) ? `${param}-01` : currentMonth();
}

export const monthToParam = (month: string) => month.slice(0, 7);
