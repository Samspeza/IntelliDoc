import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import type { RespostaAutenticacao } from "@/shared/types/api";

/**
 * Instância Axios central usada por TODAS as chamadas à Api (uma por
 * feature, em features/<nome>/api/). Duas responsabilidades:
 *
 * 1. Injeta o access token em toda requisição (interceptor de request).
 * 2. Ao receber 401 (access token expirado, vida útil de 15 minutos -
 *    Etapa 9.5), tenta renovar via /api/auth/refresh UMA ÚNICA VEZ e
 *    reencaminha a requisição original - transparente para quem chamou.
 *    Se o refresh também falhar (refresh token expirado/revogado, RN10),
 *    limpa a sessão e redireciona para o login.
 *
 * O controle de "uma única vez" evita loop infinito caso o próprio
 * endpoint de refresh comece a retornar 401.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "/api",
  headers: { "Content-Type": "application/json" }
});

const CHAVE_ACCESS_TOKEN = "intellidoc:accessToken";
const CHAVE_REFRESH_TOKEN = "intellidoc:refreshToken";

export function salvarSessao(resposta: RespostaAutenticacao) {
  localStorage.setItem(CHAVE_ACCESS_TOKEN, resposta.accessToken);
  localStorage.setItem(CHAVE_REFRESH_TOKEN, resposta.refreshToken);
}

export function limparSessao() {
  localStorage.removeItem(CHAVE_ACCESS_TOKEN);
  localStorage.removeItem(CHAVE_REFRESH_TOKEN);
}

export function obterAccessToken() {
  return localStorage.getItem(CHAVE_ACCESS_TOKEN);
}

function obterRefreshToken() {
  return localStorage.getItem(CHAVE_REFRESH_TOKEN);
}

apiClient.interceptors.request.use((config) => {
  const token = obterAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Fila de requisições que chegaram 401 enquanto um refresh já está em
// andamento - evita disparar múltiplos POST /auth/refresh simultâneos
// quando várias chamadas falham ao mesmo tempo (ex.: dashboard que dispara
// 3 queries em paralelo com o token expirado).
let refreshEmAndamento: Promise<string> | null = null;

async function renovarToken(): Promise<string> {
  const refreshToken = obterRefreshToken();

  if (!refreshToken) {
    throw new Error("Nenhum refresh token disponível.");
  }

  const resposta = await axios.post<RespostaAutenticacao>(
    `${apiClient.defaults.baseURL}/auth/refresh`,
    { refreshToken }
  );

  salvarSessao(resposta.data);
  return resposta.data.accessToken;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const requisicaoOriginal = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;

    const ehErroDeAutenticacao = error.response?.status === 401;
    const jaTentouRenovar = requisicaoOriginal?._retry === true;
    const ehRequisicaoDeRefresh = requisicaoOriginal?.url?.includes("/auth/refresh");

    if (!ehErroDeAutenticacao || jaTentouRenovar || ehRequisicaoDeRefresh || !requisicaoOriginal) {
      return Promise.reject(error);
    }

    requisicaoOriginal._retry = true;

    try {
      refreshEmAndamento ??= renovarToken().finally(() => {
        refreshEmAndamento = null;
      });

      const novoAccessToken = await refreshEmAndamento;

      requisicaoOriginal.headers.Authorization = `Bearer ${novoAccessToken}`;
      return apiClient(requisicaoOriginal);
    } catch (erroRefresh) {
      limparSessao();
      window.location.href = "/login";
      return Promise.reject(erroRefresh);
    }
  }
);