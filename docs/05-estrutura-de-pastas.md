# IntelliDoc — Estrutura de Pastas

## 1. Backend (.NET) — Solução completa

```
backend/
├── IntelliDoc.sln
├── .editorconfig
├── Directory.Build.props
├── src/
│   ├── IntelliDoc.Domain/                      # Camada mais interna. Sem dependências externas.
│   │   ├── Entities/
│   │   │   ├── Empresa.cs
│   │   │   ├── Usuario.cs
│   │   │   ├── Documento.cs
│   │   │   ├── CampoExtraido.cs
│   │   │   ├── HistoricoStatusDocumento.cs
│   │   │   ├── Notificacao.cs
│   │   │   ├── ConfiguracaoEmpresa.cs
│   │   │   └── RegistroAuditoria.cs
│   │   ├── Enums/
│   │   │   ├── StatusDocumento.cs
│   │   │   ├── TipoDocumento.cs
│   │   │   ├── PapelUsuario.cs
│   │   │   └── TipoAcaoAuditoria.cs
│   │   ├── ValueObjects/
│   │   │   ├── Cnpj.cs
│   │   │   └── ConfidenceScore.cs
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   └── IAuditavel.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── TransicaoStatusInvalidaException.cs
│   │   │   └── RegraDeNegocioException.cs
│   │   └── Events/
│   │       ├── DocumentoAprovadoEvent.cs
│   │       └── DocumentoRejeitadoEvent.cs
│   │
│   ├── IntelliDoc.Application/                 # Casos de uso (Commands/Queries + Handlers)
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   ├── ICurrentUserService.cs
│   │   │   │   ├── IDateTimeProvider.cs
│   │   │   │   ├── IDocumentExtractionService.cs   # abstração do OCR/IA
│   │   │   │   ├── IFileStorageService.cs
│   │   │   │   ├── IDocumentProcessingQueue.cs
│   │   │   │   ├── IEmailService.cs
│   │   │   │   └── ICacheService.cs
│   │   │   ├── Behaviors/                          # MediatR Pipeline Behaviors
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── UnhandledExceptionBehavior.cs
│   │   │   ├── Models/
│   │   │   │   └── PaginatedList.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs               # AutoMapper
│   │   ├── Identidade/
│   │   │   ├── Commands/
│   │   │   │   ├── RegistrarEmpresa/
│   │   │   │   ├── Login/
│   │   │   │   ├── RefreshToken/
│   │   │   │   ├── EsqueciSenha/
│   │   │   │   ├── RedefinirSenha/
│   │   │   │   ├── CriarUsuario/
│   │   │   │   └── AtualizarUsuario/
│   │   │   └── Queries/
│   │   │       ├── ObterPerfil/
│   │   │       └── ListarUsuariosEmpresa/
│   │   ├── Empresas/
│   │   │   ├── Commands/
│   │   │   │   ├── CriarEmpresa/
│   │   │   │   ├── AtivarDesativarEmpresa/
│   │   │   │   └── AtualizarConfiguracaoEmpresa/
│   │   │   └── Queries/
│   │   │       └── ListarEmpresas/
│   │   ├── Documentos/
│   │   │   ├── Commands/
│   │   │   │   ├── UploadDocumento/
│   │   │   │   ├── ProcessarDocumento/             # usado pelo Worker
│   │   │   │   ├── ReenviarDocumento/
│   │   │   │   ├── ArquivarDocumento/
│   │   │   │   ├── CorrigirCamposExtraidos/
│   │   │   │   ├── AprovarDocumento/
│   │   │   │   └── RejeitarDocumento/
│   │   │   └── Queries/
│   │   │       ├── ObterDocumentoPorId/
│   │   │       ├── ListarDocumentos/
│   │   │       └── ListarDocumentosPendentesRevisao/
│   │   ├── Dashboard/
│   │   │   └── Queries/
│   │   │       ├── ObterIndicadores/
│   │   │       └── ExportarRelatorio/
│   │   ├── Notificacoes/
│   │   │   ├── Commands/
│   │   │   │   └── MarcarNotificacaoComoLida/
│   │   │   └── Queries/
│   │   │       └── ListarNotificacoes/
│   │   └── Auditoria/
│   │       └── Queries/
│   │           └── ListarAuditoria/
│   │
│   ├── IntelliDoc.Infrastructure/               # Implementações concretas
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/                  # IEntityTypeConfiguration por entidade
│   │   │   ├── Interceptors/
│   │   │   │   └── AuditableEntitySaveChangesInterceptor.cs
│   │   │   ├── Migrations/
│   │   │   └── Seed/
│   │   │       └── ApplicationDbContextSeed.cs
│   │   ├── Identity/
│   │   │   ├── JwtTokenService.cs
│   │   │   └── CurrentUserService.cs
│   │   ├── Extraction/
│   │   │   ├── TesseractExtractionService.cs    # implementação padrão
│   │   │   └── AiFieldParsingService.cs
│   │   ├── Storage/
│   │   │   └── LocalFileStorageService.cs
│   │   ├── Queue/
│   │   │   └── HangfireDocumentProcessingQueue.cs
│   │   ├── Email/
│   │   │   └── SmtpEmailService.cs
│   │   ├── Caching/
│   │   │   └── RedisCacheService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── IntelliDoc.Api/                          # Presentation layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── EmpresasController.cs
│   │   │   ├── UsuariosController.cs
│   │   │   ├── DocumentosController.cs
│   │   │   ├── DashboardController.cs
│   │   │   ├── NotificacoesController.cs
│   │   │   └── AuditoriaController.cs
│   │   ├── Middlewares/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── CorrelationIdMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ApiExceptionFilterAttribute.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Dockerfile
│   │
│   └── IntelliDoc.Worker/                       # Background worker (processo separado)
│       ├── ProcessarDocumentoBackgroundService.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
│
└── tests/
    ├── IntelliDoc.UnitTests/
    │   ├── Domain/
    │   └── Application/
    └── IntelliDoc.IntegrationTests/
        ├── Controllers/
        └── Common/
            └── CustomWebApplicationFactory.cs
```

## 2. Frontend (React + TypeScript + Vite) — Estrutura planejada

> Criação efetiva do projeto (`npm create vite`) e destes arquivos ocorre na Etapa 10.
> Documentamos aqui para já reservar a organização por *feature folders* definida na Etapa 4.

```
frontend/
├── index.html
├── vite.config.ts
├── tailwind.config.ts
├── tsconfig.json
├── .env.example
├── Dockerfile
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── app/
│   │   ├── router.tsx
│   │   └── providers.tsx                  # QueryClientProvider, ThemeProvider, etc.
│   ├── shared/
│   │   ├── components/
│   │   │   └── ui/                        # componentes shadcn (button, dialog, table...)
│   │   ├── layouts/
│   │   │   ├── AuthLayout.tsx
│   │   │   └── AppLayout.tsx
│   │   ├── hooks/
│   │   ├── lib/
│   │   │   ├── api-client.ts              # instância axios + interceptors
│   │   │   └── utils.ts
│   │   └── types/
│   │       └── api.ts
│   ├── features/
│   │   ├── auth/
│   │   │   ├── components/                # LoginForm, RegisterForm...
│   │   │   ├── hooks/                     # useLogin, useRefreshToken...
│   │   │   ├── api/
│   │   │   └── pages/
│   │   │       ├── LoginPage.tsx
│   │   │       ├── RegisterPage.tsx
│   │   │       ├── ForgotPasswordPage.tsx
│   │   │       └── ResetPasswordPage.tsx
│   │   ├── documentos/
│   │   │   ├── components/                # UploadDropzone, DocumentoTable...
│   │   │   ├── hooks/
│   │   │   ├── api/
│   │   │   └── pages/
│   │   │       ├── DocumentosListPage.tsx
│   │   │       ├── DocumentoDetalhePage.tsx
│   │   │       └── UploadPage.tsx
│   │   ├── aprovacao/
│   │   │   ├── components/                # RevisaoLadoALado, CampoExtraidoEditor...
│   │   │   ├── hooks/
│   │   │   ├── api/
│   │   │   └── pages/
│   │   │       └── FilaRevisaoPage.tsx
│   │   ├── dashboard/
│   │   │   ├── components/                # KpiCard, GraficoTaxaAprovacao...
│   │   │   ├── hooks/
│   │   │   ├── api/
│   │   │   └── pages/
│   │   │       └── DashboardPage.tsx
│   │   ├── administracao/
│   │   │   ├── components/                # UsuariosTable, EmpresasTable...
│   │   │   ├── hooks/
│   │   │   ├── api/
│   │   │   └── pages/
│   │   │       ├── UsuariosPage.tsx
│   │   │       ├── EmpresasPage.tsx       # visão Super Admin
│   │   │       └── ConfiguracoesEmpresaPage.tsx
│   │   ├── perfil/
│   │   │   └── pages/
│   │   │       └── PerfilPage.tsx
│   │   ├── notificacoes/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   └── api/
│   │   └── auditoria/
│   │       ├── components/
│   │       └── pages/
│   │           └── AuditoriaPage.tsx
│   └── styles/
│       └── globals.css
└── public/
    └── favicon.ico
```

## 3. Justificativa das principais escolhas de organização

- **Backend por projeto físico (não só por pasta lógica):** cada camada da Clean Architecture (Domain, Application, Infrastructure, Api, Worker) é um **projeto .csproj separado** dentro da mesma solução, não apenas uma pasta dentro de um único projeto. Isso torna a regra de dependência (Etapa 4, item 2) **imposta pelo próprio compilador** — se `Domain` tentar referenciar `Infrastructure`, a build simplesmente falha, e não apenas "convenção verbal".
- **Worker como projeto executável próprio:** reforça a decisão arquitetural de processo separado (Etapa 4) — ele tem seu próprio `Program.cs`, `Dockerfile` e ciclo de vida, podendo escalar (múltiplas réplicas) independente da API.
- **Application organizada por módulo, não por tipo técnico:** dentro de `Application/`, agrupamos por domínio de negócio (`Documentos/`, `Aprovacao` fica dentro de `Documentos/Commands`, `Dashboard/`, etc.) em vez de uma pasta `Commands/` genérica com tudo misturado — isso mantém a navegação por feature, essencial num projeto deste tamanho.
- **Frontend por feature folder:** já justificado na Etapa 4 — evita a "explosão" de uma pasta `components/` genérica com dezenas de componentes não relacionados.
- **Pasta `tests/` na raiz do backend, separada de `src/`:** convenção padrão .NET, deixa claro que testes não fazem parte do artefato de deploy.

## 4. Convenção de nomenclatura

- **C#:** PascalCase para classes/métodos, `Async` como sufixo em métodos assíncronos, um Command/Query por pasta com seu Handler e Validator juntos (ex.: `AprovarDocumento/AprovarDocumentoCommand.cs`, `AprovarDocumentoCommandHandler.cs`, `AprovarDocumentoCommandValidator.cs`).
- **TypeScript/React:** PascalCase para componentes (`DocumentoTable.tsx`), camelCase para hooks (`useDocumentos.ts`), kebab-case para arquivos utilitários quando não são componentes.
- **Nomes em português para o domínio de negócio** (entidades, regras), **inglês para infraestrutura/técnico** (`ApplicationDbContext`, `IEmailService`) — mantém consistência com o padrão do próprio framework .NET.