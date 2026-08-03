# IntelliDoc — Diagrama de Classes do Domínio

## 1. Agregados (Aggregate Roots) identificados

| Aggregate Root | Entidades filhas (dentro do agregado) | Value Objects |
|---|---|---|
| `Empresa` | `ConfiguracaoEmpresa` | — |
| `Usuario` | `UsuarioPapel`, `RefreshToken` | `Email` |
| `Documento` | `CampoExtraido`, `HistoricoStatusDocumento` | `ConfidenceScore` |
| `Notificacao` | — | — |
| `RegistroAuditoria` | — | — |

**Por que `Documento` é o agregado central:** todas as regras de transição de
status (RN13 a RN24) só fazem sentido garantidas **atomicamente** — não é
possível, por exemplo, aprovar um documento sem gerar o registro de
histórico correspondente na mesma operação. Por isso `CampoExtraido` e
`HistoricoStatusDocumento` não têm repositório próprio: só são
alterados **através** do agregado `Documento`, nunca diretamente.

## 2. Diagrama de Classes (Mermaid)

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CriadoEm
        +string? CriadoPor
        +DateTime? AtualizadoEm
        +string? AtualizadoPor
    }

    class Empresa {
        +string Nome
        +string? CnpjOuIdentificador
        +bool Ativa
        +ConfiguracaoEmpresa Configuracao
        +Desativar() void
        +Ativar() void
    }

    class ConfiguracaoEmpresa {
        +Guid EmpresaId
        +decimal LimiarConfiancaIa
        +List~TipoDocumento~ TiposDocumentoAceitos
        +bool SegregacaoRevisorAtiva
        +AtualizarLimiar(decimal) void
    }

    class Usuario {
        +Guid? EmpresaId
        +string Nome
        +Email Email
        +string SenhaHash
        +bool Ativo
        +List~UsuarioPapel~ Papeis
        +List~RefreshToken~ RefreshTokens
        +TemPapel(PapelUsuario) bool
        +Desativar() void
        +AdicionarPapel(PapelUsuario) void
    }

    class Email {
        <<Value Object>>
        +string Valor
        +Criar(string) Email
    }

    class UsuarioPapel {
        +Guid UsuarioId
        +PapelUsuario Papel
    }

    class RefreshToken {
        +Guid UsuarioId
        +string TokenHash
        +DateTime ExpiraEm
        +DateTime? RevogadoEm
        +EstaValido() bool
        +Revogar() void
    }

    class Documento {
        +Guid EmpresaId
        +Guid EnviadoPorUsuarioId
        +string NomeArquivoOriginal
        +string CaminhoArmazenamento
        +TipoDocumento TipoDocumento
        +StatusDocumento Status
        +bool PrioridadeRevisao
        +ConfidenceScore? ScoreMedio
        +int TentativasProcessamento
        +string? MotivoRejeicao
        +bool Arquivado
        +List~CampoExtraido~ Campos
        +List~HistoricoStatusDocumento~ Historico
        +IniciarProcessamento() void
        +RegistrarResultadoExtracao(List~CampoExtraido~, ConfidenceScore) void
        +MarcarFalhaProcessamento() void
        +PodeReprocessar() bool
        +CorrigirCampo(string nomeCampo, string novoValor, Guid usuarioId) void
        +Aprovar(Guid usuarioId, bool segregacaoAtiva) void
        +Rejeitar(Guid usuarioId, string motivo) void
        +Arquivar() void
        -TransicionarStatus(StatusDocumento novo, Guid? usuarioId, string? motivo) void
    }

    class CampoExtraido {
        +Guid DocumentoId
        +string NomeCampo
        +string? ValorExtraidoIa
        +string? ValorFinal
        +ConfidenceScore Confidence
        +bool CorrigidoManualmente
        +Corrigir(string novoValor) void
    }

    class ConfidenceScore {
        <<Value Object>>
        +decimal Valor
        +Criar(decimal) ConfidenceScore
        +EstaAbaixoDoLimiar(decimal limiar) bool
    }

    class HistoricoStatusDocumento {
        +Guid DocumentoId
        +StatusDocumento StatusAnterior
        +StatusDocumento StatusNovo
        +Guid? UsuarioId
        +string? Motivo
    }

    class Notificacao {
        +Guid EmpresaId
        +Guid? UsuarioDestinoId
        +PapelUsuario? PapelDestino
        +string Titulo
        +string Mensagem
        +bool Lida
        +Guid? DocumentoRelacionadoId
        +MarcarComoLida() void
    }

    class RegistroAuditoria {
        +Guid? EmpresaId
        +Guid? UsuarioId
        +TipoAcaoAuditoria Acao
        +string EntidadeAfetada
        +Guid? EntidadeAfetadaId
        +string? DadosAntesJson
        +string? DadosDepoisJson
        +string EnderecoIp
    }

    BaseEntity <|-- Empresa
    BaseEntity <|-- Usuario
    BaseEntity <|-- Documento
    BaseEntity <|-- Notificacao
    BaseEntity <|-- RegistroAuditoria
    BaseEntity <|-- CampoExtraido
    BaseEntity <|-- HistoricoStatusDocumento

    Empresa "1" *-- "1" ConfiguracaoEmpresa : agrega
    Usuario "1" *-- "*" UsuarioPapel : agrega
    Usuario "1" *-- "*" RefreshToken : agrega
    Usuario "1" -- "1" Email : possui

    Documento "1" *-- "*" CampoExtraido : agrega
    Documento "1" *-- "*" HistoricoStatusDocumento : agrega
    Documento "1" -- "0..1" ConfidenceScore : possui
    CampoExtraido "1" -- "1" ConfidenceScore : possui
```

## 3. Máquina de Estados do `Documento` (regra central do domínio)

```mermaid
stateDiagram-v2
    [*] --> Enviado : UploadDocumento (UC13)
    Enviado --> Processando : Worker inicia (UC14)
    Processando --> AguardandoRevisao : extração OK (RN15)
    Processando --> FalhaProcessamento : erro no OCR/IA (RN14)
    FalhaProcessamento --> Enviado : reprocessar (RN14, máx 3x)
    AguardandoRevisao --> Aprovado : Aprovar (UC22, RN18-19)
    AguardandoRevisao --> Rejeitado : Rejeitar (UC23, RN18-19,21)
    Rejeitado --> Enviado : Reenviar (UC17, RN22)
    Aprovado --> Arquivado : Arquivar (UC18)
    Rejeitado --> Arquivado : Arquivar (UC18)
    Enviado --> Arquivado : Arquivar (UC18)
```

**Por que isso vira código:** o método privado `Documento.TransicionarStatus`
é o **único** ponto do sistema que altera `Status` — ele valida se a
transição solicitada é permitida por este diagrama (RN19) e, se não for,
lança `TransicaoStatusInvalidaException` (definida na Etapa 5). Isso
significa que é **impossível**, por construção, um documento pular de
`Enviado` direto para `Aprovado` sem passar pelos estados intermediários,
mesmo que um bug em uma camada superior tente forçar isso.

## 4. Onde cada Regra de Negócio (RN) mora no código

| Regra | Classe/Método responsável |
|---|---|
| RN02 (isolamento tenant) | Global Query Filter no `ApplicationDbContext` (Infrastructure) — fora do Domain, é preocupação de persistência |
| RN13-RN14 (fila, retry) | `Documento.IniciarProcessamento()`, `Documento.MarcarFalhaProcessamento()`, `Documento.PodeReprocessar()` |
| RN15-RN16 (score, prioridade) | `Documento.RegistrarResultadoExtracao()` + `ConfidenceScore.EstaAbaixoDoLimiar()` |
| RN17 (soft delete) | `Documento.Arquivar()` |
| RN18-RN19 (quem aprova, transição válida) | `Documento.Aprovar()` / `Documento.Rejeitar()` (validação de papel é feita na Application, via Command Handler + `ICurrentUserService`; validação de transição é feita no Domain) |
| RN20 (preserva valor original da IA) | `CampoExtraido.Corrigir()` — nunca sobrescreve `ValorExtraidoIa`, só `ValorFinal` |
| RN21 (motivo obrigatório na rejeição) | `Documento.Rejeitar(usuarioId, motivo)` — lança `DomainException` se `motivo.Length < 10` |
| RN23 (auditoria da transição) | `Documento.TransicionarStatus()` sempre adiciona um `HistoricoStatusDocumento` |
| RN24 (bloqueio de autoaprovação) | `Documento.Aprovar(usuarioId, segregacaoAtiva)` — compara `usuarioId` com `EnviadoPorUsuarioId` |

## 5. Decisão de design: por que `Aprovar`/`Rejeitar` recebem parâmetros e não dependem de Services injetados

O agregado `Documento` **não recebe nenhuma dependência injetada** (sem
`IUsuarioRepository`, sem `IConfiguracaoService` dentro dele) — em vez
disso, os dados necessários para a decisão (`segregacaoAtiva`, por
exemplo) são **passados como parâmetro** pelo Command Handler, que já os
buscou previamente. Isso mantém o Domain **puro e testável** (testes
unitários instanciam um `Documento` e chamam `Aprovar(...)` sem precisar
de mocks de infraestrutura), delegando toda orquestração externa
(consultar configuração da empresa, verificar papel do usuário) para a
camada de Application — que é o lugar correto para orquestração, segundo
Clean Architecture.