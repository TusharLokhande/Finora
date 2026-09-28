import { axiosClient } from "@/api/axiosClient";
import type { Category, CreateCategoryInput, UpdateCategoryInput } from "../types/category.types";

export async function getCategories(): Promise<Category[]> {
  const res = await axiosClient.get<Category[]>("/categories");
  return res.data;
}

export async function createCategory(payload: CreateCategoryInput): Promise<Category> {
  const res = await axiosClient.post<Category>("/categories", payload);
  return res.data;
}

export async function updateCategory(id: string, payload: UpdateCategoryInput): Promise<Category> {
  const res = await axiosClient.put<Category>(`/categories/${id}`, payload);
  return res.data;
}

export async function archiveCategory(id: string): Promise<Category> {
  const res = await axiosClient.patch<Category>(`/categories/${id}/archive`);
  return res.data;
}

export async function restoreCategory(id: string): Promise<Category> {
  const res = await axiosClient.patch<Category>(`/categories/${id}/restore`);
  return res.data;
}
