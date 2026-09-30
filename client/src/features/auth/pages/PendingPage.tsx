import { Clock } from "lucide-react";
import { StatusCard } from "../components/StatusCard";

export function PendingPage({ email }: { email: string }) {
  return (
    <StatusCard
      icon={Clock}
      tone="warning"
      title="Your request is pending"
      body="We'll email you once your account is approved. This usually doesn't take long."
      email={email}
    />
  );
}
