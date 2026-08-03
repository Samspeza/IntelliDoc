# IntelliDoc — Casos de Uso

Formato: **UC** | Ator | Ação | Pós-condição / Resultado | RFs/RNs relacionados

## Módulo 1 — Identidade e Acesso

- **UC01** | Visitante | Registrar-se como Admin de uma nova empresa (onboarding self-service) | Empresa criada + usuário AdminEmpresa criado + e-mail de boas-vindas | RF01, RF04, RN01
- **UC02** | Usuário | Autenticar-se (e-mail + senha) | Recebe access token (JWT) + refresh token | RF02, RN09, RN34
- **UC03** | Usuário | Renovar sessão via refresh token | Novo access token emitido | RF02, RN10
- **UC04** | Usuário | Solicitar recuperação de senha | E-mail com link/token de redefinição enviado | RF03
- **UC05** | Usuário | Redefinir senha via token | Senha atualizada, refresh tokens antigos invalidados | RF03, RN10
- **UC06** | Usuário autenticado | Efetuar logout | Refresh token corrente invalidado | RN10
- **UC07** | AdminEmpresa | Criar usuário na própria empresa (definindo papéis) | Usuário criado, e-mail de convite enviado | RF23, RN05, RN07
- **UC08** | AdminEmpresa | Editar papéis / desativar usuário da própria empresa | Usuário atualizado/desativado; ação auditada | RF23, RN08, RN32
- **UC09** | Usuário autenticado | Visualizar/editar o próprio perfil | Dados do perfil atualizados | RF23 (extensão)

## Módulo 2 — Empresas (Super Admin)

- **UC10** | SuperAdmin | Criar nova empresa | Empresa criada, inativa até primeiro admin ser definido ou já ativa com admin inicial | RF24, RN01
- **UC11** | SuperAdmin | Ativar/desativar empresa | Empresa (e seus usuários) bloqueada/desbloqueada para login | RF24, RN03
- **UC12** | SuperAdmin | Listar empresas com métricas resumidas (usuários, volume de documentos) | Lista paginada retornada | RF24

## Módulo 3 — Upload e Processamento de Documentos

- **UC13** | Operador | Fazer upload de um ou mais documentos | Documento(s) criado(s) com status `Enviado`, enfileirado(s) para processamento | RF06, RF07, RN11, RN12, RN13
- **UC14** | Sistema (Worker) | Processar documento da fila (OCR + extração IA) | Documento atualizado com texto OCR, campos extraídos, confidence score, status → `AguardandoRevisao` ou `FalhaProcessamento` | RF08, RF09, RF10, RN13-RN16
- **UC15** | Usuário autenticado | Consultar status/detalhe de um documento | Retorna documento, campos extraídos, histórico de status | RF11, RN02
- **UC16** | Usuário autenticado | Listar documentos com filtros (status, tipo, período) | Lista paginada, respeitando isolamento de tenant | RF11, RN02, RN25
- **UC17** | Operador ou AdminEmpresa | Reenviar documento com falha/rejeitado para reprocessamento | Documento volta para fila, status → `Enviado` | RN14, RN22
- **UC18** | AdminEmpresa | Arquivar (soft delete) um documento | Documento marcado como arquivado, oculto das listagens padrão | RF17 (suporte), RN17

## Módulo 4 — Fluxo de Aprovação

- **UC19** | Revisor/Gestor | Listar documentos pendentes de revisão (com priorização) | Lista ordenada, priorizando `RevisaoPrioritaria` | RF12, RN16
- **UC20** | Revisor/Gestor | Visualizar documento + campos extraídos lado a lado | Retorna URL/preview do arquivo + dados extraídos + confidence por campo | RF12
- **UC21** | Revisor/Gestor | Corrigir um ou mais campos extraídos | Campos atualizados; valor original da IA preservado no histórico | RF13, RN20
- **UC22** | Revisor/Gestor | Aprovar documento | Status → `Aprovado`; dados finais consolidados; auditoria gerada; notificação ao operador | RF14, RN18, RN19, RN23, RN30
- **UC23** | Revisor/Gestor | Rejeitar documento (com motivo) | Status → `Rejeitado`; auditoria gerada; notificação ao operador | RF14, RN18, RN19, RN21, RN23, RN30
- **UC24** | Sistema | Bloquear autoaprovação quando segregação estiver ativa (RN24) | Tentativa de aprovação pelo próprio autor do upload é rejeitada com erro de regra de negócio | RN24

## Módulo 5 — Dashboard e Relatórios

- **UC25** | Gestor/AdminEmpresa | Visualizar dashboard com indicadores (volume, tempo médio, taxa de aprovação, pendências) | Dados agregados retornados (com cache) | RF17, RN25, RN26
- **UC26** | Gestor/AdminEmpresa | Filtrar dashboard por período/tipo/status | Indicadores recalculados conforme filtro | RF18
- **UC27** | Gestor/AdminEmpresa | Exportar dados filtrados em CSV/Excel | Arquivo gerado (síncrono se pequeno, assíncrono se grande) para download | RF19, RN27, RN28
- **UC28** | Gestor/AdminEmpresa | Exportar relatório consolidado em PDF | PDF gerado com resumo de indicadores do período | RF20, RN27

## Módulo 6 — Notificações

- **UC29** | Sistema | Notificar revisores sobre documentos pendentes (agregado) | Notificação in-app + e-mail criada para revisores da empresa | RF21, RN29
- **UC30** | Sistema | Notificar operador sobre aprovação/rejeição do seu documento | Notificação in-app + e-mail criada para o operador | RF22, RN30
- **UC31** | Usuário autenticado | Listar/marcar notificações in-app como lidas | Notificações atualizadas | RF21, RF22, RN31

## Módulo 7 — Configurações e Auditoria

- **UC32** | AdminEmpresa | Editar configurações da empresa (limiar de confiança, tipos aceitos, segregação) | Configuração atualizada; ação auditada | RF25, RN36, RN37
- **UC33** | AdminEmpresa/SuperAdmin | Consultar trilha de auditoria (com filtros) | Lista paginada e imutável de eventos de auditoria | RF16 (suporte), RN32, RN33

## Mapeamento Caso de Uso → Camada de Aplicação (prévia)

Cada UC acima será implementado como:
- Um **Command** (para ações que alteram estado, ex.: UC13, UC22) ou uma **Query** (para leitura, ex.: UC16, UC25), seguindo o padrão CQRS via MediatR.
- Um **Handler** correspondente na camada de Application.
- Um **endpoint** no Controller correspondente da camada de API.

Essa listagem será a referência oficial ao desenhar os Controllers/Commands/Queries na Etapa 9 (Backend).