import { Label } from "@/ui/label";
import { Select } from "@/ui/select";
import { SegmentedControl } from "@/components/SegmentedControl";
import { TIMEZONES } from "../constants/settings.constants";
import { useUpdatePreferences } from "../hooks/mutations/useUpdatePreferences";
import type { DensityPreference, Settings, ThemePreference } from "../types/settings.types";

const THEME_OPTIONS: { value: ThemePreference; label: string }[] = [
  { value: "Light", label: "Light" },
  { value: "Dark", label: "Dark" },
  { value: "System", label: "System" },
];

const DENSITY_OPTIONS: { value: DensityPreference; label: string }[] = [
  { value: "Compact", label: "Compact" },
  { value: "Comfortable", label: "Comfortable" },
];

interface TimezoneOption {
  value: string;
  label: string;
}

/** Each control saves on change; only the control being saved is disabled. */
export function PreferencesTab({ settings }: { settings: Settings }) {
  const update = useUpdatePreferences();
  const saving = update.isPending ? Object.keys(update.variables)[0] : null;

  const zones = TIMEZONES.includes(settings.timezone) ? TIMEZONES : [settings.timezone, ...TIMEZONES];
  const options: TimezoneOption[] = zones.map((zone) => ({ value: zone, label: zone.replaceAll("_", " ") }));

  return (
    <div className="flex max-w-md flex-col gap-6">
      <div className="flex flex-col gap-1.5">
        <Label>Currency</Label>
        <p className="text-sm font-medium text-foreground">{settings.currencyCode}</p>
        <p className="text-xs text-muted-foreground">Set during setup. Can&apos;t be changed.</p>
      </div>

      <div className="flex flex-col gap-1.5">
        <Label htmlFor="settings-timezone">Timezone</Label>
        <Select<TimezoneOption>
          inputId="settings-timezone"
          value={options.find((o) => o.value === settings.timezone) ?? null}
          onChange={(option) => option && update.mutate({ timezone: option.value })}
          options={options}
          isDisabled={saving === "timezone"}
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>Theme</Label>
        <SegmentedControl label="Theme" value={settings.theme} options={THEME_OPTIONS} disabled={saving === "theme"} onChange={(theme) => update.mutate({ theme })} />
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>Density</Label>
        <SegmentedControl label="Density" value={settings.density} options={DENSITY_OPTIONS} disabled={saving === "density"} onChange={(density) => update.mutate({ density })} />
      </div>
    </div>
  );
}
