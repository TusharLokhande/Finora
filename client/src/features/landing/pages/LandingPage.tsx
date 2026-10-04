import { ArrowRight, ArrowRightLeft, BarChart3, Download, Landmark, PiggyBank, Tags, Wallet, type LucideIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { ThemeToggle } from "@/components/ThemeToggle";
import { GoogleIcon } from "@/features/auth/components/GoogleIcon";
import { GOOGLE_LOGIN_URL } from "@/features/auth/constants/auth.constants";
import { useAuthStore } from "@/store/authStore";
import { Button } from "@/ui/button";

const features: { icon: LucideIcon; title: string; body: string }[] = [
  { icon: ArrowRightLeft, title: "Transactions", body: "Log income and spending in seconds. Search, filter and edit anything." },
  { icon: Landmark, title: "Accounts & cards", body: "Bank accounts, wallets and credit cards, with balances that stay accurate." },
  { icon: Tags, title: "Categories", body: "Organise spending with categories and sub-categories that fit your life." },
  { icon: PiggyBank, title: "Budgets", body: "Set monthly limits and see at a glance which ones are at risk." },
  { icon: BarChart3, title: "Reports", body: "Income versus spending and top categories, over any date range." },
  { icon: Download, title: "Your data, yours", body: "Export everything whenever you like, or delete your account for good." },
];

const steps = [
  ["Sign in", "One click with Google. No passwords to manage."],
  ["Add your accounts", "Start with what you have; add the rest as you go."],
  ["Track and see", "Record transactions and watch the picture form."],
];

export function LandingPage() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  const cta = isAuthenticated ? (
    <Button asChild size="lg" className="rounded-full">
      <Link to="/home">Open dashboard <ArrowRight /></Link>
    </Button>
  ) : (
    <Button asChild size="lg" className="rounded-full">
      <a href={GOOGLE_LOGIN_URL}><GoogleIcon className="size-4" /> Get started with Google</a>
    </Button>
  );

  return (
    <div className="min-h-svh bg-background text-foreground">
      <header className="mx-auto flex h-16 max-w-5xl items-center justify-between px-4">
        <Link to="/" className="flex items-center gap-2">
          <span className="flex size-8 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <Wallet className="size-4" />
          </span>
          <span className="font-heading text-sm font-semibold">Finora</span>
        </Link>
        <div className="flex items-center gap-2">
          <ThemeToggle />
          {!isAuthenticated && (
            <Button asChild variant="ghost" size="sm"><Link to="/login">Sign in</Link></Button>
          )}
        </div>
      </header>

      <main>
        <section className="mx-auto max-w-3xl px-4 pb-20 pt-16 text-center sm:pt-24">
          <h1 className="font-heading text-4xl font-semibold tracking-tight sm:text-6xl">
            Know where your <span className="text-primary">money</span> goes.
          </h1>
          <p className="mx-auto mt-5 max-w-xl text-base text-muted-foreground sm:text-lg">
            Finora is a simple, private money manager. Track spending, set budgets and see the full picture, without the clutter.
          </p>
          <div className="mt-8 flex justify-center">{cta}</div>
          <p className="mt-3 text-xs text-muted-foreground">Free to use. Sign in with Google, no setup.</p>
        </section>

        <section className="mx-auto max-w-5xl px-4 pb-20">
          <div className="grid gap-px overflow-hidden rounded-2xl border border-border bg-border sm:grid-cols-2 lg:grid-cols-3">
            {features.map(({ icon: Icon, title, body }) => (
              <div key={title} className="bg-card p-6">
                <Icon className="size-5 text-primary" />
                <h3 className="mt-4 font-medium text-card-foreground">{title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{body}</p>
              </div>
            ))}
          </div>
        </section>

        <section className="mx-auto max-w-5xl px-4 pb-20">
          <h2 className="text-center font-heading text-2xl font-semibold">Up and running in minutes</h2>
          <ol className="mt-10 grid gap-8 sm:grid-cols-3">
            {steps.map(([title, body], i) => (
              <li key={title} className="text-center sm:text-left">
                <span className="text-sm font-medium text-primary">0{i + 1}</span>
                <h3 className="mt-1 font-medium">{title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{body}</p>
              </li>
            ))}
          </ol>
        </section>

        <section className="mx-auto max-w-3xl px-4 pb-24 text-center">
          <h2 className="font-heading text-2xl font-semibold">Ready for a clearer view?</h2>
          <div className="mt-6 flex justify-center">{cta}</div>
        </section>
      </main>

      <footer className="border-t border-border py-6 text-center text-xs text-muted-foreground">
        © {new Date().getFullYear()} Finora
      </footer>
    </div>
  );
}
