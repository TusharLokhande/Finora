import { Popover, PopoverContent, PopoverTrigger } from "@/ui/popover";
import { Button } from "@/ui/button";
import { cn } from "@/lib/utils";
import { CATEGORY_ICON_NAMES, getCategoryIcon } from "../constants/categoryIcons";

interface CategoryIconPickerProps {
  value: string | null;
  onChange: (icon: string) => void;
  color: string | null;
}

export function CategoryIconPicker({ value, onChange, color }: CategoryIconPickerProps) {
  const SelectedIcon = getCategoryIcon(value);

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button type="button" variant="outline" className="justify-start gap-2">
          <span
            className="flex size-5 items-center justify-center rounded-md"
            style={{ backgroundColor: `${color ?? "#64748b"}20`, color: color ?? "#64748b" }}
          >
            <SelectedIcon className="size-3.5" />
          </span>
          Choose icon
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-64">
        <div className="grid grid-cols-6 gap-1">
          {CATEGORY_ICON_NAMES.map((name) => {
            const Icon = getCategoryIcon(name);
            const isSelected = name === value;
            return (
              <button
                key={name}
                type="button"
                onClick={() => onChange(name)}
                aria-label={name}
                className={cn(
                  "flex size-8 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-muted hover:text-foreground",
                  isSelected && "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground",
                )}
              >
                <Icon className="size-4" />
              </button>
            );
          })}
        </div>
      </PopoverContent>
    </Popover>
  );
}
