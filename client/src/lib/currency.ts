// ponytail: currency is hardcoded to INR; the app doesn't expose the user's
// currency_code to the frontend yet (single-currency, decided at signup per docs/project-overview.md).
const CURRENCY_CODE = "INR";
const MINOR_UNITS_PER_MAJOR = 100;

const formatter = new Intl.NumberFormat("en-IN", {
  style: "currency",
  currency: CURRENCY_CODE,
  maximumFractionDigits: 2,
});

/** Formats an amount stored in minor units (paise) as a currency string, e.g. 150000 -> "₹1,500.00". */
export function formatCurrency(minorUnits: number): string {
  return formatter.format(minorUnits / MINOR_UNITS_PER_MAJOR);
}

/** Converts a major-unit amount (as entered in a form, e.g. rupees) to minor units for the API. */
export function toMinorUnits(majorUnits: number): number {
  return Math.round(majorUnits * MINOR_UNITS_PER_MAJOR);
}

/** Converts a minor-unit amount from the API to major units for display in a form input. */
export function toMajorUnits(minorUnits: number): number {
  return minorUnits / MINOR_UNITS_PER_MAJOR;
}
