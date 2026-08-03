# IntelliDoc — Regras de Negócio

## 1. Multi-tenant (Empresas)

- RN01: Todo usuário pertence a exatamente uma empresa (tenant), exceto o Super Admin, que não pertence a nenhuma e enxerga todas.
- RN02: Todo dado sensível (documento, usuário, configuração, log de auditoria) é sempre associado a um `EmpresaId`. Nenhuma consulta pode retornar dados de outra empresa que não seja a do usuário autenticado.
- RN03: Uma empresa pode ser desativada pelo Super Admin. Ao ser desativada, nenhum usuário dessa empresa consegue autenticar, mas os dados permanecem preservados (soft delete / flag `Ativa`).
- RN04: Um Admin de Empresa não pode alterar dados de configuração de outra empresa, mesmo que tente manipular o ID diretamente na requisição (validação sempre no backend, nunca confiar em ID vindo do cliente sem checar propriedade).

## 2. Usuários e Papéis (RBAC)

- RN05: Papéis possíveis: `SuperAdmin`, `AdminEmpresa`, `Gestor`, `Revisor`, `Operador`.
- RN06: Um usuário pode ter mais de um papel dentro da mesma empresa (ex.: Gestor + Revisor), mas não pertence a mais de uma empresa.
- RN07: Apenas `AdminEmpresa` pode criar/editar/desativar usuários da própria empresa. Apenas `SuperAdmin` pode criar empresas.
- RN08: Um usuário desativado não pode autenticar, mas seu histórico de ações (auditoria, aprovações passadas) é preservado.
- RN09: Senhas são armazenadas com hash (nunca em texto plano) e devem atender política mínima (8+ caracteres, letra maiúscula, número).
- RN10: Refresh token tem validade limitada e é invalidado no logout ou na troca de senha.

## 3. Upload e Processamento de Documentos

- RN11: Apenas arquivos PDF, JPG e PNG são aceitos; tamanho máximo de 10MB por arquivo.
- RN12: Todo documento enviado é vinculado ao usuário que fez o upload (`EnviadoPorUsuarioId`) e à empresa dele.
- RN13: Um documento recém-criado entra automaticamente na fila de processamento com status `Enviado`, transicionando para `Processando` quando um worker o pega.
- RN14: Se o processamento (OCR/IA) falhar, o documento vai para status `FalhaProcessamento` e pode ser reenviado manualmente para a fila (reprocessamento), até um limite de 3 tentativas automáticas.
- RN15: Após extração bem-sucedida, o documento vai para `AguardandoRevisao`, junto com os campos extraídos e o `confidence score` de cada campo.
- RN16: Se o confidence score médio do documento estiver abaixo do limiar configurado pela empresa (padrão: 70%), o documento é marcado como `RevisaoPrioritaria` (ainda dentro de `AguardandoRevisao`, mas sinalizado/priorizado nas listagens).
- RN17: Documentos não podem ser excluídos fisicamente por usuários comuns — apenas arquivados (soft delete), preservando trilha de auditoria. Exclusão física é restrita a rotinas administrativas do Super Admin.

## 4. Fluxo de Aprovação

- RN18: Apenas usuários com papel `Revisor` ou `Gestor` podem aprovar/rejeitar documentos.
- RN19: Um documento só pode ser aprovado/rejeitado quando estiver em `AguardandoRevisao`. Transições de status fora dessa ordem são bloqueadas.
- RN20: Ao aprovar, o revisor pode ter corrigido um ou mais campos extraídos; a versão corrigida é a que fica registrada como "dado final", mas o valor originalmente extraído pela IA é preservado no histórico (para métricas de acurácia da IA).
- RN21: Rejeição exige motivo obrigatório (texto livre, mínimo 10 caracteres).
- RN22: Um documento rejeitado pode ser reenviado para reprocessamento pelo operador que o enviou, ou por um Admin da Empresa.
- RN23: Toda mudança de status de um documento gera um registro de auditoria imutável (quem, quando, de qual status para qual status, e motivo quando aplicável).
- RN24: Um revisor não pode aprovar um documento enviado por si mesmo se essa segregação estiver habilitada nas configurações da empresa (regra opcional de compliance — configurável, desabilitada por padrão).

## 5. Dashboards e Relatórios

- RN25: Métricas de dashboard consideram apenas documentos da empresa do usuário autenticado (ou de todas, se Super Admin com filtro explícito de empresa).
- RN26: Taxa de aprovação = documentos aprovados / (aprovados + rejeitados) no período filtrado.
- RN27: Exportações (CSV/Excel/PDF) respeitam os mesmos filtros aplicados na tela e o mesmo isolamento de tenant.
- RN28: Exportações grandes (acima de um limite de linhas, ex. 5.000) são geradas de forma assíncrona (job) e disponibilizadas para download quando prontas, evitando timeout de requisição.

## 6. Notificações

- RN29: Revisores recebem notificação quando houver novos documentos em `AguardandoRevisao` atribuídos à sua empresa (não instantânea por documento — agregada, ex.: a cada X minutos ou ao fim do processamento em lote, para evitar spam).
- RN30: Operador recebe notificação individual quando SEU documento for aprovado ou rejeitado.
- RN31: Notificações in-app ficam disponíveis por 30 dias; e-mails não são reenviados automaticamente em caso de falha de entrega (falha é apenas logada).

## 7. Auditoria e Segurança

- RN32: Toda ação sensível (login, criação/edição de usuário, mudança de papel, aprovação/rejeição de documento, alteração de configuração da empresa, criação/desativação de empresa) gera um registro de auditoria com: usuário, ação, entidade afetada, timestamp, IP de origem.
- RN33: Registros de auditoria são imutáveis — não podem ser editados ou excluídos via API, mesmo por Super Admin.
- RN34: Rate limiting aplicado a endpoints de autenticação (login, recuperação de senha) para mitigar força bruta.
- RN35: Todo input do usuário é validado no backend (nunca confiar apenas em validação de frontend).

## 8. Configurações por Empresa

- RN36: Cada empresa pode configurar: limiar de confiança da IA (padrão 70%), tipos de documento aceitos, se a segregação revisor/operador (RN24) está ativa.
- RN37: Alterar uma configuração da empresa é uma ação auditada (RN32) e só pode ser feita por `AdminEmpresa` ou `SuperAdmin`.