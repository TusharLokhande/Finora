import axiosClient from "@/api/axiosClient";
import type { AuthUser } from "@/types/auth.types";

export async function getMe(): Promise<AuthUser> {
  const res = await axiosClient.get<AuthUser>("/auth/me");
  return res.data;
}

export function logout() {
  return axiosClient.post<void>("/auth/logout");
}
