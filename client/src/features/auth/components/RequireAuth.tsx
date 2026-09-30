import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { refreshAccessToken } from "@/api/axiosClient";
import { FullScreenLoader } from "@/components/FullScreenLoader";
import { useAuthStore } from "@/store/authStore";
import { useMe } from "../hooks/queries/useMe";
import { BlockedPage } from "../pages/BlockedPage";
import { PendingPage } from "../pages/PendingPage";

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

  return isAuthenticated ? <StatusGate /> : <Navigate to="/login" replace />;
}

/** The server decides the status; this only picks the screen. */
function StatusGate() {
  const { data: me, isPending } = useMe();

  if (isPending) return <FullScreenLoader />;
  if (me?.status === "Pending") return <PendingPage email={me.email} />;
  if (me?.status === "Rejected" || me?.status === "Suspended") return <BlockedPage status={me.status} reason={me.reason} />;

  return <Outlet />;
}
