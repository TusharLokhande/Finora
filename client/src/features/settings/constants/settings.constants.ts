/** Must match SettingsService.DeleteConfirmation on the server. */
export const DELETE_CONFIRMATION = "DELETE MY ACCOUNT";

export const SETTINGS_TABS = [
  { value: "profile", label: "Profile" },
  { value: "preferences", label: "Preferences" },
  { value: "data", label: "Data" },
] as const;

export type SettingsTab = (typeof SETTINGS_TABS)[number]["value"];

/** ponytail: no onboarding screen exists yet to share a timezone list with, so use the browser's IANA list. */
export const TIMEZONES: string[] = Intl.supportedValuesOf("timeZone");
