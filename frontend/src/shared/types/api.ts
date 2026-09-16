/**
 * Tipos que espelham os contratos da Api (backend/src/IntelliDoc.Application).
 * Mantidos manualmente por enquanto - uma evolução natural do projeto seria
 * gerar este arquivo automaticamente a partir do OpenAPI (Swagger, Etapa
 * 9.7) via `openapi-typescript`, documentado no roadmap do README.
 */

/** Espelha Application.Common.Models.RespostaAutenticacao (Etapa 9.8). */
export interface RespostaAutenticacao {
  accessToken: string;
  refreshToken: string;
  expiraEm: string;
  usuarioId: string;
  nome: string;
  email: string;
  empresaId: string | null;
  papeis: PapelUsuario[];
}

/** Espelha Domain.Enums.PapelUsuario (Etapa 9.1). */
export type PapelUsuario = "SuperAdmin" | "AdminEmpresa" | "Gestor" | "Revisor" | "Operador";

/** Espelha Domain.Enums.StatusDocumento (Etapa 9.1). */
export type StatusDocumento =
  | "Enviado"
  | "Processando"
  | "AguardandoRevisao"
  | "Aprovado"
  | "Rejeitado"
  | "FalhaProcessamento"
  | "Arquivado";

/** Espelha Application.Common.Models.PaginatedList<T> (Etapa 9.3). */
export interface PaginatedList<T> {
  itens: T[];
  paginaAtual: number;
  totalPaginas: number;
  totalItens: number;
  temPaginaAnterior: boolean;
  temProximaPagina: boolean;
}

/** Espelha o formato ProblemDetails (RFC 7807) retornado pelo ExceptionHandlingMiddleware (Etapa 9.7). */
export interface ProblemDetails {
  title: string;
  detail?: string;
  status: number;
  type?: string;
  instance?: string;
  correlationId?: string;
  errors?: Record<string, string[]>;
}