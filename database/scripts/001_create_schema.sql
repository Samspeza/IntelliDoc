-- ============================================================================
-- IntelliDoc — Script de criação de schema (PostgreSQL)
-- Etapa 8 — corresponde à modelagem definida em docs/06-modelagem-banco.md
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================================
-- 1. EMPRESAS
-- ============================================================================
CREATE TABLE "Empresas" (
    "Id"                    uuid PRIMARY KEY,
    "Nome"                  varchar(200) NOT NULL,
    "CnpjOuIdentificador"   varchar(20)  NULL,
    "Ativa"                 boolean      NOT NULL DEFAULT true,
    "CriadoEm"              timestamptz  NOT NULL DEFAULT now(),
    "CriadoPor"             varchar(200) NULL,
    "AtualizadoEm"          timestamptz  NULL,
    "AtualizadoPor"         varchar(200) NULL,
    CONSTRAINT "UQ_Empresas_Cnpj" UNIQUE ("CnpjOuIdentificador")
);

COMMENT ON TABLE "Empresas" IS 'Tenants da plataforma (RN01-RN04)';

-- ============================================================================
-- 2. CONFIGURACOES_EMPRESA (1:1 com Empresas)
-- ============================================================================
CREATE TABLE "ConfiguracoesEmpresa" (
    "Id"                        uuid PRIMARY KEY,
    "EmpresaId"                 uuid NOT NULL,
    "LimiarConfiancaIa"         decimal(5,2) NOT NULL DEFAULT 70.00,
    "TiposDocumentoAceitos"     jsonb NOT NULL DEFAULT '["NotaFiscal","Recibo","Contrato","Outro"]',
    "SegregacaoRevisorAtiva"    boolean NOT NULL DEFAULT false,
    "AtualizadoEm"              timestamptz NULL,
    "AtualizadoPor"             varchar(200) NULL,
    CONSTRAINT "FK_ConfiguracoesEmpresa_Empresas" FOREIGN KEY ("EmpresaId")
        REFERENCES "Empresas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_ConfiguracoesEmpresa_EmpresaId" UNIQUE ("EmpresaId"),
    CONSTRAINT "CK_LimiarConfianca_Range" CHECK ("LimiarConfiancaIa" BETWEEN 0 AND 100)
);

-- ============================================================================
-- 3. USUARIOS
-- ============================================================================
CREATE TABLE "Usuarios" (
    "Id"            uuid PRIMARY KEY,
    "EmpresaId"     uuid NULL,  -- nulo apenas para SuperAdmin (RN01)
    "Nome"          varchar(150) NOT NULL,
    "Email"         varchar(256) NOT NULL,
    "SenhaHash"     varchar(500) NOT NULL,
    "Ativo"         boolean NOT NULL DEFAULT true,
    "CriadoEm"      timestamptz NOT NULL DEFAULT now(),
    "CriadoPor"     varchar(200) NULL,
    "AtualizadoEm"  timestamptz NULL,
    "AtualizadoPor" varchar(200) NULL,
    CONSTRAINT "FK_Usuarios_Empresas" FOREIGN KEY ("EmpresaId")
        REFERENCES "Empresas" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "UQ_Usuarios_Email" UNIQUE ("Email")
);

-- Papel: 0=SuperAdmin, 1=AdminEmpresa, 2=Gestor, 3=Revisor, 4=Operador (RN05)
CREATE TABLE "UsuarioPapeis" (
    "Id"        uuid PRIMARY KEY,
    "UsuarioId" uuid NOT NULL,
    "Papel"     smallint NOT NULL,
    "CriadoEm"  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "FK_UsuarioPapeis_Usuarios" FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_UsuarioPapeis_Usuario_Papel" UNIQUE ("UsuarioId", "Papel"),
    CONSTRAINT "CK_UsuarioPapeis_Papel_Range" CHECK ("Papel" BETWEEN 0 AND 4)
);

CREATE TABLE "RefreshTokens" (
    "Id"          uuid PRIMARY KEY,
    "UsuarioId"   uuid NOT NULL,
    "TokenHash"   varchar(500) NOT NULL,
    "ExpiraEm"    timestamptz NOT NULL,
    "RevogadoEm"  timestamptz NULL,
    "CriadoEm"    timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "FK_RefreshTokens_Usuarios" FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

-- ============================================================================
-- 4. DOCUMENTOS
-- ============================================================================
-- TipoDocumento: 0=NotaFiscal, 1=Recibo, 2=Contrato, 3=Outro, 4=NaoClassificado
-- Status: 0=Enviado, 1=Processando, 2=AguardandoRevisao, 3=Aprovado,
--         4=Rejeitado, 5=FalhaProcessamento, 6=Arquivado
CREATE TABLE "Documentos" (
    "Id"                        uuid PRIMARY KEY,
    "EmpresaId"                 uuid NOT NULL,
    "EnviadoPorUsuarioId"       uuid NOT NULL,
    "NomeArquivoOriginal"       varchar(300) NOT NULL,
    "CaminhoArmazenamento"      varchar(500) NOT NULL,
    "TipoArquivo"               varchar(10) NOT NULL,
    "TamanhoBytes"              bigint NOT NULL,
    "TipoDocumento"             smallint NOT NULL DEFAULT 4,
    "Status"                    smallint NOT NULL DEFAULT 0,
    "PrioridadeRevisao"         boolean NOT NULL DEFAULT false,
    "ConfidenceScoreMedio"      decimal(5,2) NULL,
    "TentativasProcessamento"   int NOT NULL DEFAULT 0,
    "TextoOcrBruto"             text NULL,
    "MotivoRejeicao"            varchar(500) NULL,
    "Arquivado"                 boolean NOT NULL DEFAULT false,
    "CriadoEm"                  timestamptz NOT NULL DEFAULT now(),
    "CriadoPor"                 varchar(200) NULL,
    "AtualizadoEm"              timestamptz NULL,
    "AtualizadoPor"             varchar(200) NULL,
    CONSTRAINT "FK_Documentos_Empresas" FOREIGN KEY ("EmpresaId")
        REFERENCES "Empresas" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Documentos_UsuarioEnvio" FOREIGN KEY ("EnviadoPorUsuarioId")
        REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_Documentos_TipoArquivo" CHECK ("TipoArquivo" IN ('pdf','jpg','png')),
    CONSTRAINT "CK_Documentos_TamanhoMax" CHECK ("TamanhoBytes" <= 10485760), -- 10MB (RN11)
    CONSTRAINT "CK_Documentos_Status_Range" CHECK ("Status" BETWEEN 0 AND 6),
    CONSTRAINT "CK_Documentos_TipoDoc_Range" CHECK ("TipoDocumento" BETWEEN 0 AND 4),
    CONSTRAINT "CK_Documentos_Tentativas_Max" CHECK ("TentativasProcessamento" <= 3) -- RN14
);

CREATE TABLE "CamposExtraidos" (
    "Id"                    uuid PRIMARY KEY,
    "DocumentoId"           uuid NOT NULL,
    "NomeCampo"             varchar(100) NOT NULL,
    "ValorExtraidoIa"       varchar(500) NULL,
    "ValorFinal"            varchar(500) NULL,
    "ConfidenceScore"       decimal(5,2) NOT NULL,
    "CorrigidoManualmente"  boolean NOT NULL DEFAULT false,
    "CriadoEm"              timestamptz NOT NULL DEFAULT now(),
    "AtualizadoEm"          timestamptz NULL,
    CONSTRAINT "FK_CamposExtraidos_Documentos" FOREIGN KEY ("DocumentoId")
        REFERENCES "Documentos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_CamposExtraidos_Confidence_Range" CHECK ("ConfidenceScore" BETWEEN 0 AND 100)
);

CREATE TABLE "HistoricoStatusDocumento" (
    "Id"             uuid PRIMARY KEY,
    "DocumentoId"    uuid NOT NULL,
    "StatusAnterior" smallint NOT NULL,
    "StatusNovo"     smallint NOT NULL,
    "UsuarioId"      uuid NULL, -- nulo quando transição automática (worker)
    "Motivo"         varchar(500) NULL,
    "CriadoEm"       timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "FK_Historico_Documentos" FOREIGN KEY ("DocumentoId")
        REFERENCES "Documentos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Historico_Usuarios" FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios" ("Id") ON DELETE SET NULL
);
-- RN23/RN33: imutabilidade é garantida na Application (sem Commands de UPDATE/DELETE
-- expostos para esta tabela), não via trigger de banco, mantendo a regra visível no código.

-- ============================================================================
-- 5. NOTIFICACOES
-- ============================================================================
CREATE TABLE "Notificacoes" (
    "Id"                     uuid PRIMARY KEY,
    "EmpresaId"              uuid NOT NULL,
    "UsuarioDestinoId"       uuid NULL,
    "PapelDestino"           smallint NULL,
    "Titulo"                 varchar(200) NOT NULL,
    "Mensagem"               varchar(1000) NOT NULL,
    "Lida"                   boolean NOT NULL DEFAULT false,
    "DocumentoRelacionadoId" uuid NULL,
    "CriadoEm"               timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "FK_Notificacoes_Empresas" FOREIGN KEY ("EmpresaId")
        REFERENCES "Empresas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Notificacoes_UsuarioDestino" FOREIGN KEY ("UsuarioDestinoId")
        REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Notificacoes_Documento" FOREIGN KEY ("DocumentoRelacionadoId")
        REFERENCES "Documentos" ("Id") ON DELETE SET NULL,
    CONSTRAINT "CK_Notificacoes_DestinoObrigatorio"
        CHECK ("UsuarioDestinoId" IS NOT NULL OR "PapelDestino" IS NOT NULL)
);

-- ============================================================================
-- 6. REGISTROS_AUDITORIA
-- ============================================================================
CREATE TABLE "RegistrosAuditoria" (
    "Id"                 uuid PRIMARY KEY,
    "EmpresaId"          uuid NULL,
    "UsuarioId"          uuid NULL,
    "Acao"               smallint NOT NULL,
    "EntidadeAfetada"    varchar(100) NOT NULL,
    "EntidadeAfetadaId"  uuid NULL,
    "DadosAntes"         jsonb NULL,
    "DadosDepois"        jsonb NULL,
    "EnderecoIp"         varchar(45) NOT NULL,
    "CriadoEm"           timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "FK_Auditoria_Empresas" FOREIGN KEY ("EmpresaId")
        REFERENCES "Empresas" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Auditoria_Usuarios" FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios" ("Id") ON DELETE SET NULL
);

-- ============================================================================
-- 7. ÍNDICES (conforme docs/06-modelagem-banco.md §5)
-- ============================================================================
CREATE INDEX "IX_Documentos_Empresa_Status" ON "Documentos" ("EmpresaId", "Status");
CREATE INDEX "IX_Documentos_Empresa_Prioridade_Status" ON "Documentos" ("EmpresaId", "PrioridadeRevisao", "Status");
CREATE INDEX "IX_Documentos_Arquivado" ON "Documentos" ("Arquivado") WHERE "Arquivado" = false;
CREATE INDEX "IX_RefreshTokens_Usuario_Revogado" ON "RefreshTokens" ("UsuarioId", "RevogadoEm");
CREATE INDEX "IX_Auditoria_Empresa_CriadoEm" ON "RegistrosAuditoria" ("EmpresaId", "CriadoEm" DESC);
CREATE INDEX "IX_Notificacoes_Usuario_Lida" ON "Notificacoes" ("UsuarioDestinoId", "Lida");
CREATE INDEX "IX_CamposExtraidos_DocumentoId" ON "CamposExtraidos" ("DocumentoId");
CREATE INDEX "IX_HistoricoStatus_DocumentoId" ON "HistoricoStatusDocumento" ("DocumentoId");
CREATE INDEX "IX_UsuarioPapeis_UsuarioId" ON "UsuarioPapeis" ("UsuarioId");

-- ============================================================================
-- Fim do script 001
-- ============================================================================