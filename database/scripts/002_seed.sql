-- ============================================================================
-- IntelliDoc — Seed de dados fictícios para desenvolvimento/demonstração
-- Etapa 8 — depende de 001_create_schema.sql já aplicado
-- Senha de todos os usuários de seed: "Senha@123" (hash BCrypt fictício abaixo
-- será substituído pelo hash real gerado pelo ASP.NET Identity na Etapa 9;
-- este script serve de referência de dados, o seed definitivo roda via
-- ApplicationDbContextSeed.cs no startup em ambiente de Development)
-- ============================================================================

-- ---------------------------------------------------------------------------
-- Empresas
-- ---------------------------------------------------------------------------
INSERT INTO "Empresas" ("Id", "Nome", "CnpjOuIdentificador", "Ativa") VALUES
('11111111-1111-1111-1111-111111111111', 'Contabilidade Horizonte Ltda', '12.345.678/0001-90', true),
('22222222-2222-2222-2222-222222222222', 'Distribuidora Nova Era S.A.',  '98.765.432/0001-10', true);

INSERT INTO "ConfiguracoesEmpresa" ("Id", "EmpresaId", "LimiarConfiancaIa", "TiposDocumentoAceitos", "SegregacaoRevisorAtiva") VALUES
('a1111111-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111', 70.00, '["NotaFiscal","Recibo","Contrato","Outro"]', false),
('a2222222-0000-0000-0000-000000000002', '22222222-2222-2222-2222-222222222222', 80.00, '["NotaFiscal","Recibo"]', true);

-- ---------------------------------------------------------------------------
-- Usuários — Empresa 1 (Contabilidade Horizonte)
-- ---------------------------------------------------------------------------
INSERT INTO "Usuarios" ("Id", "EmpresaId", "Nome", "Email", "SenhaHash", "Ativo") VALUES
('b1000001-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111', 'Ana Souza (Admin)',    'ana.admin@horizonte.com',    'PLACEHOLDER_HASH', true),
('b1000002-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111111', 'Bruno Lima (Gestor)',  'bruno.gestor@horizonte.com',  'PLACEHOLDER_HASH', true),
('b1000003-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111111', 'Carla Dias (Revisora)','carla.revisora@horizonte.com','PLACEHOLDER_HASH', true),
('b1000004-0000-0000-0000-000000000004', '11111111-1111-1111-1111-111111111111', 'Diego Alves (Operador)','diego.operador@horizonte.com','PLACEHOLDER_HASH', true);

INSERT INTO "UsuarioPapeis" ("Id", "UsuarioId", "Papel") VALUES
(uuid_generate_v4(), 'b1000001-0000-0000-0000-000000000001', 1), -- AdminEmpresa
(uuid_generate_v4(), 'b1000002-0000-0000-0000-000000000002', 2), -- Gestor
(uuid_generate_v4(), 'b1000003-0000-0000-0000-000000000003', 3), -- Revisor
(uuid_generate_v4(), 'b1000004-0000-0000-0000-000000000004', 4); -- Operador

-- ---------------------------------------------------------------------------
-- Usuários — Empresa 2 (Distribuidora Nova Era) + 1 Super Admin da plataforma
-- ---------------------------------------------------------------------------
INSERT INTO "Usuarios" ("Id", "EmpresaId", "Nome", "Email", "SenhaHash", "Ativo") VALUES
('b2000001-0000-0000-0000-000000000001', '22222222-2222-2222-2222-222222222222', 'Elaine Costa (Admin)',   'elaine.admin@novaera.com',    'PLACEHOLDER_HASH', true),
('b2000002-0000-0000-0000-000000000002', '22222222-2222-2222-2222-222222222222', 'Fábio Reis (Revisor)',   'fabio.revisor@novaera.com',   'PLACEHOLDER_HASH', true),
('b2000003-0000-0000-0000-000000000003', '22222222-2222-2222-2222-222222222222', 'Gabriela Melo (Operadora)','gabriela.operadora@novaera.com','PLACEHOLDER_HASH', true),
('00000000-0000-0000-0000-000000000099', NULL, 'Super Admin Plataforma', 'superadmin@intellidoc.com', 'PLACEHOLDER_HASH', true);

INSERT INTO "UsuarioPapeis" ("Id", "UsuarioId", "Papel") VALUES
(uuid_generate_v4(), 'b2000001-0000-0000-0000-000000000001', 1), -- AdminEmpresa
(uuid_generate_v4(), 'b2000002-0000-0000-0000-000000000002', 3), -- Revisor
(uuid_generate_v4(), 'b2000003-0000-0000-0000-000000000003', 4), -- Operador
(uuid_generate_v4(), '00000000-0000-0000-0000-000000000099', 0); -- SuperAdmin

-- ---------------------------------------------------------------------------
-- Documentos (Empresa 1) — cobrindo diferentes status
-- ---------------------------------------------------------------------------
INSERT INTO "Documentos"
    ("Id", "EmpresaId", "EnviadoPorUsuarioId", "NomeArquivoOriginal", "CaminhoArmazenamento",
     "TipoArquivo", "TamanhoBytes", "TipoDocumento", "Status", "PrioridadeRevisao",
     "ConfidenceScoreMedio", "TentativasProcessamento", "Arquivado")
VALUES
-- Aguardando revisão, score normal
('c1000001-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111',
 'b1000004-0000-0000-0000-000000000004', 'nota_fiscal_junho.pdf', '/storage/11111111/nf_junho.pdf',
 'pdf', 245000, 0, 2, false, 92.50, 1, false),
-- Aguardando revisão, prioritário (score baixo)
('c1000002-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111111',
 'b1000004-0000-0000-0000-000000000004', 'recibo_manual_escaneado.jpg', '/storage/11111111/recibo1.jpg',
 'jpg', 890000, 1, 2, true, 58.00, 1, false),
-- Aprovado
('c1000003-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111111',
 'b1000004-0000-0000-0000-000000000004', 'contrato_fornecedor_x.pdf', '/storage/11111111/contrato_x.pdf',
 'pdf', 512000, 2, 3, false, 88.00, 1, false),
-- Rejeitado
('c1000004-0000-0000-0000-000000000004', '11111111-1111-1111-1111-111111111111',
 'b1000004-0000-0000-0000-000000000004', 'nota_ilegivel.jpg', '/storage/11111111/nota_ilegivel.jpg',
 'jpg', 300000, 0, 4, false, 41.00, 1, false),
-- Falha de processamento
('c1000005-0000-0000-0000-000000000005', '11111111-1111-1111-1111-111111111111',
 'b1000004-0000-0000-0000-000000000004', 'arquivo_corrompido.pdf', '/storage/11111111/corrompido.pdf',
 'pdf', 150000, 4, 5, false, NULL, 3, false);

UPDATE "Documentos" SET "MotivoRejeicao" = 'Imagem ilegível, não foi possível confirmar o valor total.'
WHERE "Id" = 'c1000004-0000-0000-0000-000000000004';

-- ---------------------------------------------------------------------------
-- Campos extraídos (documento c1000001 — Nota Fiscal aguardando revisão)
-- ---------------------------------------------------------------------------
INSERT INTO "CamposExtraidos" ("Id", "DocumentoId", "NomeCampo", "ValorExtraidoIa", "ValorFinal", "ConfidenceScore", "CorrigidoManualmente") VALUES
(uuid_generate_v4(), 'c1000001-0000-0000-0000-000000000001', 'CnpjFornecedor', '12.345.678/0001-90', '12.345.678/0001-90', 95.00, false),
(uuid_generate_v4(), 'c1000001-0000-0000-0000-000000000001', 'DataEmissao',    '2026-06-15',          '2026-06-15',          94.00, false),
(uuid_generate_v4(), 'c1000001-0000-0000-0000-000000000001', 'ValorTotal',     '1250.00',             '1250.00',             88.50, false);

-- ---------------------------------------------------------------------------
-- Histórico de status
-- ---------------------------------------------------------------------------
INSERT INTO "HistoricoStatusDocumento" ("Id", "DocumentoId", "StatusAnterior", "StatusNovo", "UsuarioId", "Motivo") VALUES
(uuid_generate_v4(), 'c1000003-0000-0000-0000-000000000003', 2, 3, 'b1000003-0000-0000-0000-000000000003', NULL),
(uuid_generate_v4(), 'c1000004-0000-0000-0000-000000000004', 2, 4, 'b1000003-0000-0000-0000-000000000003',
 'Imagem ilegível, não foi possível confirmar o valor total.');

-- ---------------------------------------------------------------------------
-- Notificações
-- ---------------------------------------------------------------------------
INSERT INTO "Notificacoes" ("Id", "EmpresaId", "UsuarioDestinoId", "PapelDestino", "Titulo", "Mensagem", "Lida", "DocumentoRelacionadoId") VALUES
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', NULL, 3,
 'Novos documentos para revisão', '2 documentos aguardando sua revisão.', false, NULL),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'b1000004-0000-0000-0000-000000000004', NULL,
 'Documento aprovado', 'Seu documento "contrato_fornecedor_x.pdf" foi aprovado.', false,
 'c1000003-0000-0000-0000-000000000003');

-- ============================================================================
-- Fim do seed
-- ============================================================================