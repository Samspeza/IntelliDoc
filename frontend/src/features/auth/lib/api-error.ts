import axios from "axios";
import type { ProblemDetails } from "@/shared/types/api";

/**
 * Normaliza um erro do Axios (que sempre vem como ProblemDetails do
 * ExceptionHandlingMiddleware, Etapa 9.7) em uma estrutura simples:
 * - mensagemGeral: para exibir em um alerta no topo do formulário
 *   (ex.: "Credenciais inválidas" do LoginCommandHandler, Etapa 9.8)
 * - errosPorCampo: para mapear de volta aos campos via setError do
 *   React Hook Form (formato ValidationProblemDetails.errors)
 */
export interface ErroApiNormalizado {
  mensagemGeral: string;
  errosPorCampo: Record<string, string[]>;
}

export function normalizarErroApi(error: unknown): ErroApiNormalizado {
  if (axios.isAxiosError<ProblemDetails>(error) && error.response?.data) {
    const problema = error.response.data;

    return {
      mensagemGeral: problema.detail || problema.title || "Ocorreu um erro inesperado.",
      errosPorCampo: problema.errors ?? {}
    };
  }

  return {
    mensagemGeral: "Não foi possível conectar ao servidor. Tente novamente.",
    errosPorCampo: {}
  };
}