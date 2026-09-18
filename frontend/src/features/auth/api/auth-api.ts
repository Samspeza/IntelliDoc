import { apiClient } from "@/shared/lib/api-client";
import type { RespostaAutenticacao } from "@/shared/types/api";
import type {
  EsqueciSenhaFormValues,
  LoginFormValues,
  RedefinirSenhaFormValues,
  RegistrarEmpresaFormValues
} from "@/features/auth/lib/schemas";

/**
 * Chamadas HTTP puras do módulo de autenticação - sem estado, sem cache.
 * O estado de servidor (loading/error/success) é responsabilidade dos
 * hooks em features/auth/hooks/, que envolvem estas funções com
 * useMutation (React Query).
 */
export const authApi = {
  login: (dados: LoginFormValues) =>
    apiClient.post<RespostaAutenticacao>("/auth/login", dados).then((r) => r.data),

  registrarEmpresa: (dados: Omit<RegistrarEmpresaFormValues, "confirmarSenha">) =>
    apiClient.post<RespostaAutenticacao>("/auth/registrar", dados).then((r) => r.data),

  // UC04 - endpoint ainda pendente de implementação no backend (ver nota da
  // Etapa 10.2); a chamada já está pronta para quando o Command existir.
  esqueciSenha: (dados: EsqueciSenhaFormValues) =>
    apiClient.post<void>("/auth/esqueci-senha", dados).then((r) => r.data),

  // UC05 - idem.
  redefinirSenha: (dados: RedefinirSenhaFormValues) =>
    apiClient.post<void>("/auth/redefinir-senha", dados).then((r) => r.data),

  logout: (refreshToken: string) =>
    apiClient.post<void>("/auth/logout", { refreshToken }).then((r) => r.data)
};