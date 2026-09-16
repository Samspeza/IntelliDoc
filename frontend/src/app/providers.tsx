import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";

/**
 * Instância única do QueryClient. staleTime de 30s evita refetch agressivo
 * ao trocar de aba/voltar o foco na janela (comportamento padrão do React
 * Query é refetchOnWindowFocus=true, o que gera muitas chamadas
 * desnecessárias em telas como o Dashboard). retry:1 evita insistir demais
 * em erros 4xx (que não se resolvem por tentar de novo) - erros de rede
 * ainda se beneficiam de uma segunda tentativa.
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: 1
    }
  }
});

export function AppProviders({ children }: { children: ReactNode }) {
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}