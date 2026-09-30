export const QUICK_ENTRY_AMOUNT_ID = "quick-entry-amount";

/** Focuses the quick-entry amount field (header button, `N` shortcut). */
export function focusQuickEntry() {
  const input = document.getElementById(QUICK_ENTRY_AMOUNT_ID);
  input?.scrollIntoView({ block: "center", behavior: "smooth" });
  input?.focus();
}
