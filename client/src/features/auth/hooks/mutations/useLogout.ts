import { useMutation } from "@tanstack/react-query";
import { logout } from "@/features/auth/api/auth.api";
import { useAuthStore } from "@/store/authStore";

export function useLogout() {
  return useMutation({
    mutationFn: logout,
    onSettled: () => {
      useAuthStore.getState().clearAuth();
      window.location.href = "/login";
    },
  });
}
