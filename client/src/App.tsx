import { BrowserRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/api/queryClient";
import { NotFoundPage } from "@/components/NotFoundPage";
import { ComingSoonPage } from "@/components/ComingSoonPage";
import { RootLayout } from "@/components/layout/RootLayout";
import { AuthCallbackPage, LoginPage, RequireAuth } from "@/features/auth";
import { AccountsPage } from "@/features/accounts";
import { CategoriesPage } from "@/features/categories";
import { useAppliedTheme } from "@/hooks/useAppliedTheme";
import { TooltipProvider } from "@/ui/tooltip";
import { Toaster } from "@/ui/sonner";

function App() {
  useAppliedTheme();

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Toaster />
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/auth/callback" element={<AuthCallbackPage />} />
            <Route element={<RequireAuth />}>
              <Route path="/" element={<RootLayout />}>
                <Route index element={<ComingSoonPage title="Home" />} />
                <Route path="transactions" element={<ComingSoonPage title="Transactions" />} />
                <Route path="accounts" element={<AccountsPage />} />
                <Route path="categories" element={<CategoriesPage />} />
                <Route path="reports" element={<ComingSoonPage title="Reports" />} />
                <Route path="budgets" element={<ComingSoonPage title="Budgets" />} />
              </Route>
            </Route>
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  );
}

export default App;
