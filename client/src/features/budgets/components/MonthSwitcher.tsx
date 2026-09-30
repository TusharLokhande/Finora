import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/ui/button";
import { monthLabel, shiftMonth } from "../constants/months";

export function MonthSwitcher({ month, onChange }: { month: string; onChange: (month: string) => void }) {
  return (
    <div className="flex items-center justify-center gap-2">
      <Button variant="ghost" size="icon-sm" aria-label="Previous month" onClick={() => onChange(shiftMonth(month, -1))}>
        <ChevronLeft />
      </Button>
      <span className="min-w-36 text-center text-sm font-medium text-foreground" aria-live="polite">
        {monthLabel(month)}
      </span>
      <Button variant="ghost" size="icon-sm" aria-label="Next month" onClick={() => onChange(shiftMonth(month, 1))}>
        <ChevronRight />
      </Button>
    </div>
  );
}
