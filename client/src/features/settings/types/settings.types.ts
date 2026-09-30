export type ThemePreference = "Light" | "Dark" | "System";
export type DensityPreference = "Compact" | "Comfortable";

export interface Settings {
  name: string;
  email: string;
  currencyCode: string;
  timezone: string;
  theme: ThemePreference;
  density: DensityPreference;
}

export interface Preferences {
  timezone: string;
  theme: ThemePreference;
  density: DensityPreference;
}
