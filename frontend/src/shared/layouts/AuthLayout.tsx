import { Outlet } from "react-router-dom";

/**
 * Layout das telas não autenticadas (login, registro, recuperação de
 * senha). Centraliza o conteúdo em um card sobre um fundo neutro - não tem
 * sidebar nem header, ao contrário de AppLayout.
 */
export function AuthLayout() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/30 px-4">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold tracking-tight">IntelliDoc</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Processamento inteligente de documentos
          </p>
        </div>

        <div className="rounded-lg border bg-card p-8 shadow-sm">
          <Outlet />
        </div>
      </div>
    </div>
  );
}