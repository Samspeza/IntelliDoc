import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { authApi } from "@/features/auth/api/auth-api";
import { salvarSessao } from "@/shared/lib/api-client";
import type { RegistrarEmpresaFormValues } from "@/features/auth/lib/schemas";

/**
 * UC01. Assim como no login, o backend já retorna os tokens no cadastro
 * (RegistrarEmpresaCommandHandler, Etapa 9.8) - o usuário cai direto na
 * área autenticada, sem precisar fazer login logo em seguida.
 */
export function useRegistrarEmpresa() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (dados: RegistrarEmpresaFormValues) => {
      const { confirmarSenha: _confirmarSenha, ...payload } = dados;
      return authApi.registrarEmpresa(payload);
    },
    onSuccess: (resposta) => {
      salvarSessao(resposta);
      navigate("/documentos", { replace: true });
    }
  });
}