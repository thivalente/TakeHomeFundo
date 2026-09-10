# Backend Handoff — FTH-005

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_proxima_microentrega
- **Microentregas autorizadas:** —
- **Última aprovação humana:** Gate A — planejamento

## Planejamento global

### Use case: Background Event Worker

- **Objetivo:** processar eventos `ApplicationApproved` persistidos na Outbox e entregá-los ao serviço externo sem bloquear a requisição HTTP original.
- **Entrada:** `OutboxMessage` elegível, com payload contendo `eventId`, `customerId`, `applicationId` e operação `Created` ou `Updated`.
- **Saída:** mensagem `Processed` após HTTP 200 e confirmação do mesmo `LockId`, ou `Pending`/`Failed` conforme a política de retry.
- **Regras principais:** polling a cada 5 segundos; claim atômico; lease de 30 segundos; commit antes do HTTP; timeout de 5 segundos; backoff de 5 segundos; terceira falha vira `Failed`; dados ausentes também viram `Failed`; entrega é at-least-once.
- **Dados trafegados e transformados:** IDs do evento são usados para buscar Customer/Application atuais; esses dados são convertidos em payload pela porta de integração; `eventId` permanece no envio.
- **BC / jornada / passo:** Loan Application / pós-aprovação / entrega assíncrona do evento.
- **Patterns obrigatórios:** `BackgroundService`, transactional outbox, portas de Application, escopo DI por iteração, claim com lease e atualização condicional por `LockId`.
- **Arquivos esperados:** projeto `FundoTakeHome.Worker` em `01- Hosts`; projeto compartilhado `FundoTakeHome.Backend` em `02- Backend`; portas e serviço de Application; estados/contratos necessários; persistência da Outbox; cliente de integração sem endpoint específico do FTH-006; registros nos hosts; testes de Application e Domain; documentação do handoff/checklist.
- **Arquivos proibidos:** frontend, Rule Engine, alteração do fluxo de aprovação do FTH-002, implementação do mock/endpoint do FTH-006, broker, Polly, circuit breaker, dead-letter queue e replay manual.
- **Escopo autenticado e ownership:** autenticação não se aplica; não existe usuário autenticado neste desafio.
- **Identificador real de escopo/ownership:** `CustomerId` e `ApplicationId` vêm do evento; não são identificadores de autorização.
- **Onde o identificador nasce:** no FTH-002, ao criar o `ApprovedApplicationEvent` junto com Customer, Application e OutboxMessage.
- **Contexto autenticado ou de execução:** não há contexto autenticado; o worker usa os IDs persistidos na mensagem.
- **Lookup adicional:** somente Customer e Application pelos IDs do evento; não propor lookup de ownership.
- **Motivo do lookup:** montar o payload com dados atuais e detectar referências inconsistentes. Não buscar por SSN nem duplicar regra de criação/atualização do FTH-002.

### Dependências e decisões pendentes

- O modelo atual da Outbox ainda possui apenas o estado `Pending`; serão avaliadas as alterações mínimas para os estados e campos exigidos pelo card.
- O contrato concreto do endpoint externo permanece fora deste card; o worker dependerá de uma interface de integração que o FTH-006 poderá implementar/adaptar.
- A estratégia de claim deve respeitar a capacidade transacional real do SQLite e não manter transação aberta durante o HTTP.

## BDD Coverage Matrix - backend

| BDD ID | Cenário | Criticidade | Cobertura | Tipo | Evidência | Status | Justificativa |
|---|---|---:|---|---|---|---|---|
| FTH-005-BDD-001 | Worker hospedado na API consulta a Outbox a cada 5 segundos | critical | inspeção estática do hosted service e registro | manual | pendente: path exato do worker e registro no `Program.cs` | blocking_gap | Será preenchido na microentrega 1.1. |
| FTH-005-BDD-002 | Claim muda `Pending` para `Processing` atomically e atribui `LockId`/`LockedUntil` | critical | inspeção da operação de claim e build | manual | pendente: método de claim e validação estática | blocking_gap | Não criar teste de Infrastructure. Será preenchido na microentrega 1.5. |
| FTH-005-BDD-003 | Lease dura 30 segundos e mensagem expirada pode ser reivindicada novamente | critical | teste de Application e inspeção da persistência | xUnit/manual | pendente: teste no serviço e evidência da persistência | blocking_gap | Testar comportamento em Application; não criar teste próprio de Infrastructure. |
| FTH-005-BDD-004 | Transação de claim termina antes da chamada externa | critical | inspeção estática e teste de Application quando aplicável | manual/xUnit | pendente: evidência literal da ordem commit → HTTP | blocking_gap | Não criar teste de Infrastructure. Será preenchido nas microentregas 1.4/1.5. |
| FTH-005-BDD-005 | Customer e Application atuais são carregados pelos IDs do evento | critical | teste do caso de uso | xUnit | pendente: teste com dados atuais | blocking_gap | Será preenchido na microentrega 1.4. |
| FTH-005-BDD-006 | Operações `Created` e `Updated` chamam a integração com operação correta | critical | teste do caso de uso | xUnit | pendente: testes de ambas as operações | blocking_gap | Será preenchido na microentrega 1.4. |
| FTH-005-BDD-007 | HTTP 200 marca `Processed` apenas com status e `LockId` ainda válidos | critical | teste de Application | xUnit | pendente: teste de lock-safe completion | blocking_gap | Será coberto no projeto de testes existente, sem teste de Infrastructure. |
| FTH-005-BDD-008 | Timeout, erro de rede e non-200 registram erro, incrementam tentativa e agendam 5 segundos | critical | teste de Application | xUnit | pendente: testes de falhas e `NextAttemptAt` | blocking_gap | Será coberto no projeto de testes existente. |
| FTH-005-BDD-009 | Terceira falha marca `Failed` e interrompe retries automáticos | critical | teste de Application | xUnit | pendente: teste da terceira tentativa | blocking_gap | Será coberto no projeto de testes existente. |
| FTH-005-BDD-010 | Customer/Application ausente marca a mensagem como `Failed` | critical | teste de Application | xUnit | pendente: teste com dados ausentes | blocking_gap | Será coberto no projeto de testes existente. |
| FTH-005-BDD-011 | `Attempts`, `LastError`, `ProcessedAt`, `LockedUntil` e `LockId` são registrados corretamente | critical | inspeção da entidade/configuração e testes | manual/xUnit | pendente: paths exatos e asserts | blocking_gap | Será preenchido na microentrega 1.4/1.5. |
| FTH-005-BDD-012 | Evento inclui `eventId` e entrega é at-least-once | critical | inspeção do payload e fluxo | manual | pendente: evidência literal do payload e retry após falha de conclusão | blocking_gap | Será preenchido na microentrega 1.4/1.5. |
| FTH-005-BDD-013 | Não são adicionados Polly, broker ou infraestrutura desnecessária | normal | inspeção estática dos arquivos alterados | manual | pendente: revisão final do diff | blocking_gap | Será preenchido no gate 1.6. |

## Gates executados

- Gate A: PASS — estrutura aprovada; Worker separado, Backend compartilhado e testes limitados a Application/Domain.
- Micro-gates: ainda não executados.
- Fable consolidado: reservado para o fechamento da etapa.

## Pendências e riscos

- Definir a menor alteração compatível com SQLite para claim atômico e atualização condicional.
- Confirmar a fronteira da interface de integração sem antecipar o contrato do FTH-006.
- Manter todos os testes criados e compiláveis; a execução da suíte fica para o usuário.
