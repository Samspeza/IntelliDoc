import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { authApi } from "@/features/auth/api/auth-api";
import { salvarSessao } from "@/shared/lib/api-client";

/**
 * UC02. Ao ter sucesso, persiste os tokens (localStorage, via
 * api-client.ts) e navega para a área autenticada. O tratamento de erro
 * (credenciais inválidas, rate limit 429) fica a cargo do componente que
 * consome este hook, via a propriedade `error` retornada pelo useMutation -
 * mantendo o hook livre de qualquer JSX/apresentação.
 */
export function useLogin() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: authApi.login,
    onSuccess: (resposta) => {
      salvarSessao(resposta);
      navigate("/documentos", { replace: true });
    }
  });
}