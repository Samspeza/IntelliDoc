import { Navigate, Outlet } from "react-router-dom";
import { obterAccessToken } from "@/shared/lib/api-client";

/**
 * Protege rotas que exigem autenticação. Checagem simples de presença do
 * access token no localStorage - NÃO valida expiração aqui (isso é
 * responsabilidade do interceptor de resposta do api-client, que renova
 * silenciosamente ou redireciona para /login se o refresh falhar). Esta
 * checagem evita apenas o "flash" de uma tela autenticada para quem nunca
 * fez login.
 */
export function RotaProtegida() {
  const autenticado = Boolean(obterAccessToken());

  if (!autenticado) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}