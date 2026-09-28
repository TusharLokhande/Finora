import { ShieldCheck, Wallet } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import { Button } from "@/ui/button";
import { GoogleIcon } from "@/features/auth/components/GoogleIcon";
import { ACCESS_REQUEST_MAILTO, GOOGLE_LOGIN_URL } from "@/features/auth/constants/auth.constants";

export function LoginPage() {
  const [searchParams] = useSearchParams();
  const hasError = searchParams.has("error");

  return (
    <div className="relative isolate flex min-h-svh items-center justify-center overflow-hidden bg-background px-4">
      <style>{`
        @keyframes login-line-drift { 0% { transform: translateX(0); } 100% { transform: translateX(-3%); } }
        @media (prefers-reduced-motion: no-preference) {
          .login-chart-line { animation: login-line-drift 12s ease-in-out infinite alternate; }
        }
      `}</style>
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10 overflow-hidden">
        <svg
          className="login-chart-line absolute inset-0 h-full w-full"
          viewBox="0 0 1600 900"
          preserveAspectRatio="none"
          fill="none"
        >
          <path
            d="M -50,700 C 150,740 300,600 412,562 C 550,520 650,460 800,470 C 950,480 1080,430 1220,400 C 1380,365 1500,260 1650,120"
            stroke="var(--primary)"
            strokeOpacity="0.35"
            strokeWidth="2.5"
            vectorEffect="non-scaling-stroke"
            style={{ filter: "drop-shadow(0 0 6px color-mix(in oklch, var(--primary), transparent 45%))" }}
          />
        </svg>
        <div className="absolute size-2.5 -translate-x-1/2 -translate-y-1/2 rounded-full bg-primary/70" style={{ left: "25.75%", top: "62.4%" }} />
        <div className="absolute -translate-x-1/2 -translate-y-1/2" style={{ left: "76.25%", top: "44.4%" }}>
          <span className="absolute inset-0 size-2.5 animate-ping rounded-full bg-primary/60" />
          <span className="relative block size-2.5 rounded-full bg-primary" />
        </div>
      </div>

      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center gap-3 text-center">
          <div className="flex size-12 items-center justify-center rounded-2xl bg-primary text-primary-foreground shadow-lg shadow-primary/30 ring-4 ring-primary/15">
            <Wallet className="size-6" />
          </div>
          <h1 className="font-heading text-xl font-semibold text-foreground">Finora</h1>
        </div>

        <div className="rounded-3xl border border-border bg-card p-8 shadow-xl shadow-black/5">
          <h2 className="text-center text-xl font-semibold text-card-foreground">Sign in to Finora</h2>
          <p className="mt-1 text-center text-sm text-muted-foreground">
            See where your money&apos;s going, all in one place.
          </p>

          {hasError && (
            <p className="mt-4 rounded-lg bg-destructive/10 px-3 py-2 text-center text-xs text-destructive">
              We couldn&apos;t sign you in. Your Google account may not be approved yet.
            </p>
          )}

          <Button asChild variant="outline" size="lg" className="mt-6 w-full rounded-full bg-background">
            <a href={GOOGLE_LOGIN_URL}>
              <GoogleIcon className="size-4" />
              Continue with Google
            </a>
          </Button>
          <p className="mt-3 text-center text-xs text-muted-foreground">
            We only use your Google account to verify it&apos;s you.
          </p>

          <div className="my-6 flex items-center gap-3">
            <div className="h-px flex-1 bg-border" />
            <span className="text-xs text-muted-foreground">new here</span>
            <div className="h-px flex-1 bg-border" />
          </div>

          <Button asChild variant="secondary" size="lg" className="w-full rounded-full">
            <a href={ACCESS_REQUEST_MAILTO}>
              <ShieldCheck />
              Ask for approval
            </a>
          </Button>
        </div>

        <p className="mt-6 text-center text-xs text-muted-foreground">
          Finora is invite-only. Access requests are reviewed by the team.
        </p>
      </div>
    </div>
  );
}
