import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { meKey } from "@/features/auth";
import { useAuthStore } from "@/store/authStore";
import type { ApiError } from "@/types/apiError.types";
import { updateProfile } from "../../api/settings.api";
import { settingsKeys } from "../queries/useSettings";
import type { Settings } from "../../types/settings.types";

export function useUpdateProfile() {
  const queryClient = useQueryClient();

  return useMutation<Settings, ApiError, string>({
    mutationFn: updateProfile,
    onSuccess: (settings) => {
      queryClient.setQueryData(settingsKeys.all, settings);
      queryClient.invalidateQueries({ queryKey: meKey });
      // the header menu reads the name from the auth store
      useAuthStore.setState((s) => ({ user: s.user && { ...s.user, name: settings.name } }));
      toast.success("Profile updated");
    },
    onError: (error) => toast.error(error.message ?? "Couldn't update your profile."),
  });
}
