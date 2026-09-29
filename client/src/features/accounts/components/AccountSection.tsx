import type { ReactNode } from "react";

interface AccountSectionProps {
  title: string;
  emptyMessage: string;
  isEmpty: boolean;
  children: ReactNode;
}

export function AccountSection({ title, emptyMessage, isEmpty, children }: AccountSectionProps) {
  return (
    <div className="flex flex-col gap-2">
      <h2 className="text-sm font-medium text-muted-foreground">{title}</h2>
      <div className="rounded-xl border border-border bg-card">
        {isEmpty ? (
          <p className="px-3 py-6 text-center text-sm text-muted-foreground">{emptyMessage}</p>
        ) : (
          <div className="divide-y divide-border">{children}</div>
        )}
      </div>
    </div>
  );
}
