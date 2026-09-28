import { z } from "zod";

export const categoryTypeSchema = z.enum(["Expense", "Income"]);

export const createCategorySchema = z.object({
  name: z.string().trim().min(1, "Name is required").max(200, "Name is too long"),
  type: categoryTypeSchema,
  parentId: z.uuid().nullable(),
  color: z
    .string()
    .regex(/^#[0-9a-fA-F]{6}$/, "Enter a valid hex color")
    .nullable(),
  icon: z.string().nullable(),
});

export type CreateCategoryFormValues = z.infer<typeof createCategorySchema>;
