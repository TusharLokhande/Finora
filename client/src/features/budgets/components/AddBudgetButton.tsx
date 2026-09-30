import { useState } from "react";
import { Plus } from "lucide-react";
import { Button } from "@/ui/button";
import { Select } from "@/ui/select";
import { Popover, PopoverContent, PopoverTrigger } from "@/ui/popover";
import type { UnbudgetedOption } from "../hooks/queries/useUnbudgetedCategories";

interface AddBudgetButtonProps {
  options: UnbudgetedOption[];
  onPick: (option: UnbudgetedOption) => void;
}

/** "+ Add budget": pick an expense category without a budget this month, then type the amount inline in its row. */
export function AddBudgetButton({ options, onPick }: AddBudgetButtonProps) {
  const [open, setOpen] = useState(false);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button disabled={options.length === 0} title={options.length === 0 ? "Every category has a budget" : undefined}>
          <Plus /> Add budget
        </Button>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-72 p-2">
        <Select<UnbudgetedOption>
          autoFocus
          menuIsOpen
          placeholder="Choose a category…"
          options={options}
          value={null}
          onChange={(o) => {
            if (!o) return;
            setOpen(false);
            onPick(o);
          }}
          formatOptionLabel={(o) =>
            o.stub.parentId ? <span className="pl-4 text-muted-foreground">{o.stub.categoryName}</span> : o.label
          }
          // Render the list inline inside the popover instead of floating over it.
          styles={{ menu: (base) => ({ ...base, position: "static", boxShadow: "none" }) }}
        />
      </PopoverContent>
    </Popover>
  );
}
