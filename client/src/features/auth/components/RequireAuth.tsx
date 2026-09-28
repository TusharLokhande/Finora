import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { refreshAccessToken } from "@/api/axiosClient";
import { FullScreenLoader } from "@/components/FullScreenLoader";
import { useAuthStore } from "@/store/authStore";

export function RequireAuth() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const [checked, setChecked] = useState(isAuthenticated);

  useEffect(() => {
    if (isAuthenticated) return;
    refreshAccessToken()
      .catch(() => undefined)
      .finally(() => setChecked(true));
  }, [isAuthenticated]);

  if (!checked) return <FullScreenLoader />;

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}
