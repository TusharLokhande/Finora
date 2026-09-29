import { Hammer } from "lucide-react";

export function ComingSoonPage({ title }: { title: string }) {
  return (
    <div className="flex flex-1 items-center justify-center px-4 py-16">
      <div className="w-full max-w-sm text-center">
        <div className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-primary text-primary-foreground shadow-lg shadow-primary/30 ring-4 ring-primary/15">
          <Hammer className="size-6" />
        </div>
        <h1 className="mt-6 text-xl font-semibold text-foreground">{title}</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          This page is coming soon.
        </p>
      </div>
    </div>
  );
}
