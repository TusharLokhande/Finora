import { useState } from "react";
import { Avatar, AvatarFallback } from "@/ui/avatar";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { Label } from "@/ui/label";
import { initials } from "@/lib/utils";
import { useUpdateProfile } from "../hooks/mutations/useUpdateProfile";
import type { Settings } from "../types/settings.types";

export function ProfileTab({ settings }: { settings: Settings }) {
  const [name, setName] = useState(settings.name);
  const update = useUpdateProfile();

  const trimmed = name.trim();
  const canSave = trimmed !== "" && trimmed !== settings.name && !update.isPending;

  return (
    <form
      className="flex max-w-md flex-col gap-5"
      onSubmit={(e) => {
        e.preventDefault();
        if (canSave) update.mutate(trimmed);
      }}
    >
      <Avatar size="lg">
        <AvatarFallback>{initials(settings.name)}</AvatarFallback>
      </Avatar>

      <div className="flex flex-col gap-1.5">
        <Label htmlFor="settings-name">Name</Label>
        <Input id="settings-name" value={name} maxLength={200} onChange={(e) => setName(e.target.value)} disabled={update.isPending} />
      </div>

      <div className="flex flex-col gap-1.5">
        <Label htmlFor="settings-email">Email</Label>
        <Input id="settings-email" value={settings.email} disabled readOnly />
        <p className="text-xs text-muted-foreground">Managed by your Google account.</p>
      </div>

      <Button type="submit" className="w-fit" disabled={!canSave}>Save changes</Button>
    </form>
  );
}
