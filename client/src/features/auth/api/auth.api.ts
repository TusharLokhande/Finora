import axiosClient from "@/api/axiosClient";

export function logout() {
  return axiosClient.post<void>("/auth/logout");
}
