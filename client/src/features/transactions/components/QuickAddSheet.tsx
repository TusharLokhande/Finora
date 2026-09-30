import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle } from "@/ui/sheet";
import { QuickEntryForm } from "./QuickEntryForm";

interface QuickAddSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/** The Transactions page's quick-add form in a sheet, for screens without the ledger. Stays open for the next entry. */
export function QuickAddSheet({ open, onOpenChange }: QuickAddSheetProps) {
  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Add transaction</SheetTitle>
          <SheetDescription>Enter saves and keeps the form open for the next one.</SheetDescription>
        </SheetHeader>
        <div className="px-4">
          <QuickEntryForm variant="stacked" />
        </div>
      </SheetContent>
    </Sheet>
  );
}
