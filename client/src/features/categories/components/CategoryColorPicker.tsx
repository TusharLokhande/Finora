import { Popover, PopoverContent, PopoverTrigger } from "@/ui/popover";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { cn } from "@/lib/utils";
import { CATEGORY_COLORS } from "../constants/categoryColors";

interface CategoryColorPickerProps {
  value: string | null;
  onChange: (color: string) => void;
}

export function CategoryColorPicker({ value, onChange }: CategoryColorPickerProps) {
  const color = value ?? "#64748b";

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button type="button" variant="outline" className="justify-start gap-2">
          <span className="size-4 rounded-full border border-border" style={{ backgroundColor: color }} />
          Choose color
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-64">
        <div className="grid grid-cols-8 gap-1.5">
          {CATEGORY_COLORS.map((swatch) => (
            <button
              key={swatch}
              type="button"
              onClick={() => onChange(swatch)}
              aria-label={swatch}
              className={cn(
                "size-6 rounded-full ring-offset-2 ring-offset-popover transition-shadow",
                swatch === value && "ring-2 ring-foreground",
              )}
              style={{ backgroundColor: swatch }}
            />
          ))}
        </div>
        <Input
          value={color}
          onChange={(e) => onChange(e.target.value)}
          placeholder="#3b82f6"
          className="mt-3"
        />
      </PopoverContent>
    </Popover>
  );
}
