import { Monitor, Moon, Sun } from "lucide-react";
import { Button } from "@/ui/button";
import { useUpdatePreferences } from "@/features/settings";
import { useThemeStore, type Theme } from "@/store/themeStore";

const nextTheme: Record<Theme, Theme> = {
  light: "dark",
  dark: "system",
  system: "light",
};

const icon: Record<Theme, typeof Sun> = {
  light: Sun,
  dark: Moon,
  system: Monitor,
};

const label: Record<Theme, string> = {
  light: "Light theme",
  dark: "Dark theme",
  system: "System theme",
};

export function ThemeToggle() {
  const theme = useThemeStore((s) => s.theme);
  const update = useUpdatePreferences(); // saves to the server too, otherwise the synced server value would revert this
  const Icon = icon[theme];

  return (
    <Button
      variant="outline"
      size="icon"
      aria-label={`${label[theme]}. Click to switch.`}
      onClick={() => {
        const next = nextTheme[theme];
        update.mutate({ theme: (next[0].toUpperCase() + next.slice(1)) as "Light" | "Dark" | "System" });
      }}
    >
      <Icon />
    </Button>
  );
}
