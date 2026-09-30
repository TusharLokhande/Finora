import type { LucideIcon } from "lucide-react";
import { useLogout } from "../hooks/mutations/useLogout";
import { cn } from "@/lib/utils";

interface StatusCardProps {
  icon: LucideIcon;
  tone: "warning" | "danger";
  title: string;
  body: string;
  /** Muted extra line under the body, e.g. a reason. */
  note?: string | null;
  email?: string;
}

const TONES = {
  warning: "bg-amber-500/15 text-amber-600 dark:text-amber-400",
  danger: "bg-destructive/10 text-destructive",
};

/** Full-page card shared by the Pending and Blocked screens; same look as the Login card. */
export function StatusCard({ icon: Icon, tone, title, body, note, email }: StatusCardProps) {
  const logout = useLogout();

  return (
    <div className="flex min-h-svh items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm rounded-3xl border border-border bg-card p-8 text-center shadow-xl shadow-black/5">
        <div className={cn("mx-auto flex size-14 items-center justify-center rounded-full", TONES[tone])}>
          <Icon className="size-6" />
        </div>
        <h1 className="mt-5 text-xl font-semibold text-card-foreground">{title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">{body}</p>
        {note && <p className="mt-3 text-xs text-muted-foreground/80">{note}</p>}
        {email && (
          <p className="mx-auto mt-5 w-fit max-w-full truncate rounded-full border border-border bg-muted px-3 py-1 text-xs text-muted-foreground">
            {email}
          </p>
        )}
        <button
          type="button"
          onClick={() => logout.mutate()}
          className="mt-6 text-sm text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
        >
          Sign out
        </button>
      </div>
    </div>
  );
}
