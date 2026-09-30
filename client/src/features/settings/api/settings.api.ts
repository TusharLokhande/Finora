import { axiosClient } from "@/api/axiosClient";
import type { Preferences, Settings } from "../types/settings.types";

export async function getSettings(): Promise<Settings> {
  const res = await axiosClient.get<Settings>("/settings");
  return res.data;
}

export async function updateProfile(name: string): Promise<Settings> {
  const res = await axiosClient.put<Settings>("/settings/profile", { name });
  return res.data;
}

export async function updatePreferences(preferences: Preferences): Promise<Settings> {
  const res = await axiosClient.put<Settings>("/settings/preferences", preferences);
  return res.data;
}

/** Downloads the JSON export, using the server's file name. */
export async function exportData(): Promise<void> {
  const res = await axiosClient.get<Blob>("/settings/export", { responseType: "blob" });
  const disposition = String(res.headers["content-disposition"] ?? "");
  const name = /filename="?([^";]+)"?/.exec(disposition)?.[1] ?? "finora-export.json";
  const url = URL.createObjectURL(res.data);
  Object.assign(document.createElement("a"), { href: url, download: name }).click();
  URL.revokeObjectURL(url);
}

export async function deleteAccount(confirmation: string): Promise<void> {
  await axiosClient.delete("/settings/account", { data: { confirmation } });
}
