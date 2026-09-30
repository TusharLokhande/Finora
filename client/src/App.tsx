import { BrowserRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/api/queryClient";
import { NotFoundPage } from "@/components/NotFoundPage";
import { RootLayout } from "@/components/layout/RootLayout";
import { AuthCallbackPage, LoginPage, RequireAuth } from "@/features/auth";
import { AccountsPage } from "@/features/accounts";
import { CategoriesPage } from "@/features/categories";
import { TransactionsPage } from "@/features/transactions";
import { HomePage } from "@/features/dashboard";
import { BudgetsPage } from "@/features/budgets";
import { ReportsPage } from "@/features/reports";
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
                <Route index element={<HomePage />} />
                <Route path="transactions" element={<TransactionsPage />} />
                <Route path="accounts" element={<AccountsPage />} />
                <Route path="categories" element={<CategoriesPage />} />
                <Route path="reports" element={<ReportsPage />} />
                <Route path="budgets" element={<BudgetsPage />} />
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
