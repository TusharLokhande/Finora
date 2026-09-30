import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useDensityStore } from "@/store/densityStore";
import { useThemeStore } from "@/store/themeStore";
import type { ApiError } from "@/types/apiError.types";
import { updatePreferences } from "../../api/settings.api";
import { settingsKeys } from "../queries/useSettings";
import type { Preferences, Settings } from "../../types/settings.types";

/** One preference at a time: merges the patch into the current values, applies it right away, rolls back on failure. */
export function useUpdatePreferences() {
  const queryClient = useQueryClient();

  return useMutation<Settings, ApiError, Partial<Preferences>, { previous?: Settings }>({
    mutationFn: (patch) => {
      const current = queryClient.getQueryData<Settings>(settingsKeys.all);
      if (!current) throw new Error("Settings haven't loaded yet.");
      return updatePreferences({ timezone: current.timezone, theme: current.theme, density: current.density, ...patch });
    },
    onMutate: async (patch) => {
      await queryClient.cancelQueries({ queryKey: settingsKeys.all });
      const previous = queryClient.getQueryData<Settings>(settingsKeys.all);
      if (previous) queryClient.setQueryData<Settings>(settingsKeys.all, { ...previous, ...patch });
      if (patch.theme) useThemeStore.getState().setTheme(patch.theme.toLowerCase() as "light" | "dark" | "system");
      if (patch.density) useDensityStore.getState().setDensity(patch.density.toLowerCase() as "compact" | "comfortable");
      return { previous };
    },
    onError: (error, _patch, context) => {
      if (context?.previous) queryClient.setQueryData(settingsKeys.all, context.previous);
      toast.error(error.message ?? "Couldn't save that change.");
    },
    onSuccess: () => toast.success("Saved"),
    onSettled: () => queryClient.invalidateQueries({ queryKey: settingsKeys.all }),
  });
}
