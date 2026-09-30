import { useState } from "react";
import { RefreshCw } from "lucide-react";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { Button } from "@/ui/button";
import { Skeleton } from "@/ui/skeleton";
import { cn } from "@/lib/utils";
import { DataTab } from "../components/DataTab";
import { PreferencesTab } from "../components/PreferencesTab";
import { ProfileTab } from "../components/ProfileTab";
import { SETTINGS_TABS, type SettingsTab } from "../constants/settings.constants";
import { useSettings } from "../hooks/queries/useSettings";

export function SettingsPage() {
  const [tab, setTab] = useState<SettingsTab>("profile");
  const { data: settings, isLoading, isError, refetch } = useSettings();

  return (
    <div className="flex w-full flex-col gap-5 p-4 md:p-6">
      <PageBreadcrumb items={["Settings"]} />
      <div className="space-y-1">
        <h1 className="font-heading text-xl font-semibold text-foreground">Settings</h1>
        <p className="text-sm text-muted-foreground">Manage your profile, preferences and data.</p>
      </div>

      <div className="flex flex-col gap-4 md:flex-row md:gap-6">
        <nav aria-label="Settings sections" className="flex gap-1 md:w-44 md:shrink-0 md:flex-col">
          {SETTINGS_TABS.map((t) => (
            <button
              key={t.value}
              type="button"
              aria-current={tab === t.value ? "page" : undefined}
              onClick={() => setTab(t.value)}
              className={cn(
                "rounded-lg px-3 py-1.5 text-left text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground",
                tab === t.value && "bg-muted font-medium text-foreground",
              )}
            >
              {t.label}
            </button>
          ))}
        </nav>

        <section className="min-w-0 flex-1 rounded-xl border border-border bg-card p-4 md:p-6">
          {isLoading ? (
            <div className="flex max-w-md flex-col gap-4">
              <Skeleton className="size-10 rounded-full" />
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-full" />
            </div>
          ) : isError || !settings ? (
            <div className="flex items-center justify-between gap-2 rounded-lg bg-destructive/10 px-3 py-2 text-sm text-destructive">
              Couldn&apos;t load your settings.
              <Button variant="outline" size="sm" onClick={() => refetch()}>
                <RefreshCw /> Retry
              </Button>
            </div>
          ) : tab === "profile" ? (
            <ProfileTab settings={settings} />
          ) : tab === "preferences" ? (
            <PreferencesTab settings={settings} />
          ) : (
            <DataTab />
          )}
        </section>
      </div>
    </div>
  );
}
