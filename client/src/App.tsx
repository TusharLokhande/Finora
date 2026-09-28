import { BrowserRouter } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/api/queryClient";
import { RootLayout } from "@/components/layout/RootLayout";
import { useAppliedTheme } from "@/hooks/useAppliedTheme";
import { TooltipProvider } from "@/ui/tooltip";

function App() {
  useAppliedTheme();

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <BrowserRouter>
          <RootLayout />
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  );
}

export default App;
