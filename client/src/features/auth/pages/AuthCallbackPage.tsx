import { useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { refreshAccessToken } from "@/api/axiosClient";
import { FullScreenLoader } from "@/components/FullScreenLoader";

export function AuthCallbackPage() {
  const navigate = useNavigate();
  const ran = useRef(false);

  useEffect(() => {
    if (ran.current) return;
    ran.current = true;

    refreshAccessToken()
      .then(() => navigate("/", { replace: true }))
      .catch(() => navigate("/login", { replace: true }));
  }, [navigate]);

  return <FullScreenLoader />;
}
