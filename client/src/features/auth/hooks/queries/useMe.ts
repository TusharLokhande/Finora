import { useQuery } from "@tanstack/react-query";
import { getMe } from "../../api/auth.api";

export const meKey = ["auth", "me"] as const;

/** Re-checked every minute so a suspension shows up on an open session without a new sign-in. */
export function useMe() {
  return useQuery({
    queryKey: meKey,
    queryFn: getMe,
    refetchInterval: 60 * 1000,
    refetchOnWindowFocus: true,
  });
}
