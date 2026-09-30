import { useState } from "react";
import { Button } from "@/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/ui/dialog";
import { Input } from "@/ui/input";
import { Label } from "@/ui/label";
import { DELETE_CONFIRMATION } from "../constants/settings.constants";
import { useDeleteAccount } from "../hooks/mutations/useDeleteAccount";

export function DeleteAccountDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const [phrase, setPhrase] = useState("");
  const remove = useDeleteAccount();

  function handleOpenChange(next: boolean) {
    if (remove.isPending) return;
    if (!next) setPhrase("");
    onOpenChange(next);
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault();
            if (phrase === DELETE_CONFIRMATION) remove.mutate(phrase);
          }}
        >
          <DialogHeader>
            <DialogTitle>Delete your account?</DialogTitle>
            <DialogDescription>
              This is permanent. Every account, transaction, category and budget you&apos;ve created will be removed, and it can&apos;t be recovered.
            </DialogDescription>
          </DialogHeader>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="delete-confirmation">
              Type <span className="font-mono font-semibold">{DELETE_CONFIRMATION}</span> to confirm
            </Label>
            <Input id="delete-confirmation" value={phrase} onChange={(e) => setPhrase(e.target.value)} autoComplete="off" />
          </div>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => handleOpenChange(false)} disabled={remove.isPending}>Cancel</Button>
            <Button type="submit" variant="destructive" disabled={phrase !== DELETE_CONFIRMATION || remove.isPending}>
              Delete permanently
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
