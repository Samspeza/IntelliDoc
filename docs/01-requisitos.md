# IntelliDoc — Levantamento de Requisitos

## 1. Visão Geral

O IntelliDoc é uma plataforma multiempresa (multi-tenant) para processamento
inteligente de documentos. Usuários fazem upload de documentos (PDF/imagem),
o sistema extrai dados via OCR + IA, os dados extraídos passam por um fluxo
de aprovação (revisão humana), e os documentos aprovados alimentam relatórios,
dashboards e exportações.

Caso de uso principal: empresas que recebem grande volume de notas fiscais,
recibos, contratos ou formulários e hoje fazem a digitação manual dos dados.

## 2. Personas

| Persona | Descrição | Necessidades |
|---|---|---|
| Operador | Faz upload dos documentos no dia a dia | Upload rápido, feedback do status de processamento |
| Revisor/Aprovador | Confere os dados extraídos pela IA | Interface de revisão lado a lado (documento x dados extraídos), aprovar/rejeitar/corrigir |
| Gestor Financeiro | Acompanha volumes, pendências, relatórios | Dashboards, exportação de dados, filtros |
| Administrador da Empresa | Gerencia usuários e configurações da própria empresa | Controle de usuários, papéis, permissões |
| Administrador da Plataforma (Super Admin) | Gerencia empresas (tenants) da plataforma | Criação/gestão de empresas, monitoramento geral |

## 3. Requisitos Funcionais (RF)

### Autenticação e Acesso
- RF01: Usuário deve poder se cadastrar e fazer login com e-mail/senha.
- RF02: Autenticação via JWT (access token + refresh token).
- RF03: Recuperação de senha por e-mail.
- RF04: Suporte a múltiplas empresas (multi-tenant) — cada usuário pertence a uma empresa.
- RF05: Controle de acesso baseado em papéis (RBAC): Operador, Revisor, Gestor, Admin da Empresa, Super Admin.

### Upload e Processamento de Documentos
- RF06: Upload de documentos em PDF, JPG, PNG (múltiplos arquivos por vez).
- RF07: Documento passa por fila de processamento assíncrono (background job).
- RF08: Extração de texto via OCR.
- RF09: Extração estruturada de dados (campos-chave: datas, valores, CNPJ/CPF, nome do fornecedor, itens) via IA sobre o texto do OCR.
- RF10: Classificação automática do tipo de documento (nota fiscal, recibo, contrato, outro).
- RF11: Status do documento visível em tempo real: `Enviado` → `Processando` → `Aguardando Revisão` → `Aprovado`/`Rejeitado`.

### Fluxo de Aprovação
- RF12: Revisor visualiza documento original ao lado dos dados extraídos.
- RF13: Revisor pode corrigir campos extraídos manualmente.
- RF14: Revisor pode aprovar ou rejeitar (com motivo).
- RF15: Documentos com baixa confiança de extração (score abaixo de limiar) são sinalizados para revisão prioritária.
- RF16: Histórico de todas as alterações e decisões (auditoria) por documento.

### Dashboards e Relatórios
- RF17: Dashboard com indicadores: volume processado, tempo médio de processamento, taxa de aprovação/rejeição, pendências.
- RF18: Filtros por período, tipo de documento, status, empresa (se super admin).
- RF19: Exportação de dados em CSV/Excel.
- RF20: Exportação de relatório consolidado em PDF.

### Notificações
- RF21: Notificação (in-app e e-mail) ao revisor quando houver documentos pendentes.
- RF22: Notificação ao operador quando o documento for aprovado/rejeitado.

### Administração
- RF23: Admin da empresa gerencia usuários e papéis da própria empresa.
- RF24: Super Admin gerencia empresas (criação, ativação/desativação).
- RF25: Configurações por empresa (ex.: limiar de confiança da IA, tipos de documento aceitos).

## 4. Requisitos Não Funcionais (RNF)

- RNF01: Arquitetura em camadas (Clean Architecture) no backend.
- RNF02: API RESTful documentada via Swagger/OpenAPI.
- RNF03: Processamento assíncrono via fila (background jobs) para não bloquear o upload.
- RNF04: Cache (Redis) para dados de dashboard e configurações frequentes.
- RNF05: Logs estruturados (Serilog) e observabilidade (correlação de requisições).
- RNF06: Auditoria completa de ações sensíveis (quem fez o quê e quando).
- RNF07: Segurança: hashing de senha, validação de entrada, proteção contra upload malicioso (validação de tipo/tamanho de arquivo), rate limiting.
- RNF08: Isolamento de dados entre empresas (multi-tenant) — nenhuma empresa acessa dados de outra.
- RNF09: Aplicação deve rodar via Docker Compose (API, frontend, banco, Redis, storage).
- RNF10: Testes automatizados (unitários e de integração) nas regras de negócio críticas.
- RNF11: Frontend responsivo (desktop e mobile).
- RNF12: Escalabilidade horizontal do worker de processamento (fila permite múltiplos consumidores).

## 5. Escopo da IA/OCR (importante)

Para manter o projeto executável em ambiente local/demonstração, a extração de
dados usará um serviço de OCR (ex.: Tesseract, ou API de IA multimodal para
extração estruturada). A arquitetura será desenhada com uma camada de
abstração (`IDocumentExtractionService`) para que o provedor de IA/OCR possa
ser trocado (Tesseract local, Azure Document Intelligence, API de modelo de
linguagem multimodal, etc.) sem impactar o restante do sistema.

## 6. Fora de Escopo (v1)

- Assinatura eletrônica de documentos.
- Integração contábil/fiscal direta (ex.: envio para SEFAZ).
- Aplicativo mobile nativo (apenas web responsivo).
- Múltiplos idiomas de interface (apenas PT-BR na v1, arquitetura permite i18n futuro).

## 7. Glossário

- **Tenant**: empresa cliente da plataforma, com dados isolados.
- **Confidence score**: grau de confiança da IA na extração de um campo.
- **Job**: unidade de trabalho assíncrona (ex.: processar um documento).