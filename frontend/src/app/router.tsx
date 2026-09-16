import { createBrowserRouter, Navigate, RouterProvider } from "react-router-dom";
import { AuthLayout } from "@/shared/layouts/AuthLayout";
import { AppLayout } from "@/shared/layouts/AppLayout";
import { RotaProtegida } from "@/shared/components/RotaProtegida";

/**
 * PLACEHOLDER temporário: cada uma destas telas será substituída pela
 * página real da respectiva feature nas próximas sub-etapas (10.2 em
 * diante - features/auth, features/documentos, features/aprovacao,
 * features/dashboard, etc., conforme a estrutura definida na Etapa 5).
 * Mantê-las aqui, inline, permite que o router e a navegação da sidebar
 * (AppLayout, Etapa 10.1) já sejam testáveis nesta sub-etapa, sem depender
 * de features que ainda não existem.
 */
function PlaceholderPagina({ titulo }: { titulo: string }) {
  return (
    <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
      <p className="text-sm">Página "{titulo}" será implementada em uma próxima sub-etapa.</p>
    </div>
  );
}

const router = createBrowserRouter([
  {
    element: <AuthLayout />,
    children: [
      { path: "/login", element: <PlaceholderPagina titulo="Login" /> },
      { path: "/registrar", element: <PlaceholderPagina titulo="Registrar Empresa" /> },
      { path: "/esqueci-senha", element: <PlaceholderPagina titulo="Recuperar Senha" /> },
      { path: "/redefinir-senha", element: <PlaceholderPagina titulo="Redefinir Senha" /> }
    ]
  },
  {
    element: <RotaProtegida />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <Navigate to="/documentos" replace /> },
          { path: "/documentos", element: <PlaceholderPagina titulo="Documentos" /> },
          { path: "/documentos/:id", element: <PlaceholderPagina titulo="Detalhe do Documento" /> },
          { path: "/revisao", element: <PlaceholderPagina titulo="Fila de Revisão" /> },
          { path: "/dashboard", element: <PlaceholderPagina titulo="Dashboard" /> },
          { path: "/administracao/usuarios", element: <PlaceholderPagina titulo="Usuários" /> },
          { path: "/administracao/empresas", element: <PlaceholderPagina titulo="Empresas" /> },
          { path: "/administracao/configuracoes", element: <PlaceholderPagina titulo="Configurações da Empresa" /> },
          { path: "/notificacoes", element: <PlaceholderPagina titulo="Notificações" /> },
          { path: "/auditoria", element: <PlaceholderPagina titulo="Auditoria" /> },
          { path: "/perfil", element: <PlaceholderPagina titulo="Perfil" /> }
        ]
      }
    ]
  },
  { path: "*", element: <Navigate to="/documentos" replace /> }
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}