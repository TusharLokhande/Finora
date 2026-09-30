import { ShieldOff } from "lucide-react";
import type { UserStatus } from "@/types/auth.types";
import { StatusCard } from "../components/StatusCard";

export function BlockedPage({ status, reason }: { status: UserStatus; reason: string | null }) {
  const rejected = status === "Rejected";

  return (
    <StatusCard
      icon={ShieldOff}
      tone="danger"
      title={rejected ? "Access request declined" : "Your access has been suspended"}
      body={rejected ? "Your request to join Finora wasn't approved." : "You can't use Finora right now."}
      note={reason}
    />
  );
}
