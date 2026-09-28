import { BrowserRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/api/queryClient";
import { NotFoundPage } from "@/components/NotFoundPage";
import { RootLayout } from "@/components/layout/RootLayout";
import { AuthCallbackPage, LoginPage, RequireAuth } from "@/features/auth";
import { CategoriesPage } from "@/features/categories";
import { useAppliedTheme } from "@/hooks/useAppliedTheme";
import { TooltipProvider } from "@/ui/tooltip";

function App() {
  useAppliedTheme();

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/auth/callback" element={<AuthCallbackPage />} />
            <Route element={<RequireAuth />}>
              <Route path="/" element={<RootLayout />}>
                <Route path="categories" element={<CategoriesPage />} />
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
