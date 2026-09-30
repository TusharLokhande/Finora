/**
 * Shared color states for "how much of a limit is used" (card utilization, budgets).
 * Names match the backend BudgetStatus values lowercased, so a status maps straight to a tone.
 */
export type UsageTone = "normal" | "warning" | "over";

export const USAGE_TONE_BAR: Record<UsageTone, string> = {
  normal: "bg-primary",
  warning: "bg-amber-500",
  over: "bg-destructive",
};

export const USAGE_TONE_TEXT: Record<UsageTone, string> = {
  normal: "text-muted-foreground",
  warning: "text-amber-600 dark:text-amber-400",
  over: "text-negative",
};

/** Tinted background + matching foreground, e.g. an icon badge. */
export const USAGE_TONE_SOFT: Record<UsageTone, string> = {
  normal: "bg-primary/15 text-primary",
  warning: "bg-amber-500/15 text-amber-600 dark:text-amber-400",
  over: "bg-destructive/15 text-destructive",
};

/** Backend BudgetStatus values; lowercased they are the matching tone. */
export type BudgetStatus = "Normal" | "Warning" | "Over";

export const toneFor = (status: BudgetStatus) => status.toLowerCase() as UsageTone;
