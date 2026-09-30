import { useSyncPreferences } from "@/features/settings";
import { Outlet } from "react-router-dom";
import { SidebarInset, SidebarProvider } from "@/ui/sidebar";
import { AppSidebar } from "@/components/layout/AppSidebar";
import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";

export function RootLayout() {
  useSyncPreferences();

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <Header />
        <main className="flex flex-1 flex-col overflow-auto">
          <Outlet />
        </main>
        <Footer />
      </SidebarInset>
    </SidebarProvider>
  );
}
