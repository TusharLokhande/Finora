import type { z } from "zod";
import type { categoryTypeSchema, createCategorySchema } from "../schemas/category.schema";

export type CategoryType = z.infer<typeof categoryTypeSchema>;

/** Mirrors backend CategoryDto (Application/Features/Categories/Dto/CategoryDto.cs). */
export interface Category {
  id: string;
  parentId: string | null;
  name: string;
  type: CategoryType;
  color: string | null;
  icon: string | null;
  sortOrder: number;
  active: boolean;
  defaultKey: string | null;
  children: Category[];
}

export type CreateCategoryInput = z.infer<typeof createCategorySchema>;

export interface UpdateCategoryInput {
  name: string;
  color: string | null;
  icon: string | null;
}
