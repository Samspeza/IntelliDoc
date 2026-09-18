import { createBrowserRouter, Navigate, RouterProvider } from "react-router-dom";
import { AuthLayout } from "@/shared/layouts/AuthLayout";
import { AppLayout } from "@/shared/layouts/AppLayout";
import { RotaProtegida } from "@/shared/components/RotaProtegida";
import LoginPage from "@/features/auth/pages/LoginPage";
import RegisterPage from "@/features/auth/pages/RegisterPage";
import ForgotPasswordPage from "@/features/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "@/features/auth/pages/ResetPasswordPage";

/**
 * PLACEHOLDER temporário: as páginas dos módulos ainda não implementados
 * (Documentos, Aprovação, Dashboard, Administração, Notificações,
 * Auditoria, Perfil) serão substituídas pelas páginas reais nas próximas
 * sub-etapas. A feature `auth` (Etapa 10.2) já não usa mais este
 * placeholder - ver as rotas de autenticação abaixo.
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
      { path: "/login", element: <LoginPage /> },
      { path: "/registrar", element: <RegisterPage /> },
      { path: "/esqueci-senha", element: <ForgotPasswordPage /> },
      { path: "/redefinir-senha", element: <ResetPasswordPage /> }
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