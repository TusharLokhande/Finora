import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAuthStore } from "@/store/authStore";
import type { ApiError } from "@/types/apiError.types";
import { deleteAccount } from "../../api/settings.api";

export function useDeleteAccount() {
  return useMutation<void, ApiError, string>({
    mutationFn: deleteAccount,
    onSuccess: () => {
      // The server already cleared the refresh cookie; a full navigation also drops every cached query.
      useAuthStore.getState().clearAuth();
      window.location.href = "/login?deleted=1";
    },
    onError: (error) => toast.error(error.message ?? "Couldn't delete your account."),
  });
}
