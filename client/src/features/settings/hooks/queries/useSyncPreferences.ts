import { useEffect } from "react";
import { useDensityStore } from "@/store/densityStore";
import { useThemeStore } from "@/store/themeStore";
import { useSettings } from "./useSettings";

/** The server copy of theme/density wins: pushes it into the client stores whenever it loads or changes. */
export function useSyncPreferences() {
  const { data } = useSettings();
  const setTheme = useThemeStore((s) => s.setTheme);
  const setDensity = useDensityStore((s) => s.setDensity);

  useEffect(() => {
    if (!data) return;
    setTheme(data.theme.toLowerCase() as "light" | "dark" | "system");
    setDensity(data.density.toLowerCase() as "compact" | "comfortable");
  }, [data, setTheme, setDensity]);
}
