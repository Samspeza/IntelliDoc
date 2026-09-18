import { useMutation } from "@tanstack/react-query";
import { authApi } from "@/features/auth/api/auth-api";

/**
 * UC04. Não navega automaticamente em onSuccess - a página
 * (ForgotPasswordPage) decide exibir uma mensagem de confirmação inline,
 * já que o backend (RN31-adjacente) não deve confirmar se o e-mail existe
 * ou não na base (mesma lógica de "resposta genérica" do LoginCommandHandler,
 * Etapa 9.8) - a UI trata sucesso e "e-mail não encontrado" da mesma forma.
 */
export function useEsqueciSenha() {
  return useMutation({
    mutationFn: authApi.esqueciSenha
  });
}