# Backend Handoff — FTH-005

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_fechamento_final
- **Microentregas autorizadas:** —
- **Última aprovação humana:** autorização microentrega 1.1–1.6

## Planejamento global

### Use case: Background Event Worker

- **Objetivo:** processar eventos `ApplicationApproved` persistidos na Outbox e entregá-los ao serviço externo sem bloquear a requisição HTTP original.
- **Entrada:** `OutboxMessage` elegível, com payload contendo `eventId`, `customerId`, `applicationId` e operação `Created` ou `Updated`.
- **Saída:** mensagem `Processed` após HTTP 200 e confirmação do mesmo `LockId`, ou `Pending`/`Failed` conforme a política de retry.
- **Regras principais:** polling a cada 5 segundos; claim atômico; lease de 30 segundos; commit antes do HTTP; timeout de 5 segundos; backoff de 5 segundos; terceira falha vira `Failed`; dados ausentes também viram `Failed`; entrega é at-least-once.
- **Dados trafegados e transformados:** IDs do evento são usados para buscar Customer/Application atuais; esses dados são convertidos em payload pela porta de integração; `eventId` permanece no envio.
- **BC / jornada / passo:** Loan Application / pós-aprovação / entrega assíncrona do evento.
- **Patterns obrigatórios:** `BackgroundService`, transactional outbox, portas de Application, escopo DI por iteração, claim com lease e controle de concorrência por `LockId`.
- **Arquivos esperados:** projeto `FundoTakeHome.Api` em `01- Presentation`, contendo os boundaries de Domain, Application e Infrastructure; processamento em background; persistência da Outbox; cliente de integração sem endpoint específico do FTH-006; testes de Application e Domain; documentação do handoff/checklist.
- **Arquivos proibidos:** frontend, Rule Engine, alteração do fluxo de aprovação do FTH-002, implementação do mock/endpoint do FTH-006, broker, Polly, circuit breaker, dead-letter queue e replay manual.
- **Escopo autenticado e ownership:** autenticação não se aplica; não existe usuário autenticado neste desafio.
- **Identificador real de escopo/ownership:** `CustomerId` e `ApplicationId` vêm do evento; não são identificadores de autorização.
- **Onde o identificador nasce:** no FTH-002, ao criar o `ApprovedApplicationEvent` junto com Customer, Application e OutboxMessage.
- **Contexto autenticado ou de execução:** não há contexto autenticado; o worker usa os IDs persistidos na mensagem.
- **Lookup adicional:** somente Customer e Application pelos IDs do evento; não propor lookup de ownership.
- **Motivo do lookup:** montar o payload com dados atuais e detectar referências inconsistentes. Não buscar por SSN nem duplicar regra de criação/atualização do FTH-002.

### Implementação e decisões

- A Outbox possui os estados `Pending`, `Processing`, `Processed` e `Failed`, além dos campos de lease e controle de erro exigidos pelo processamento.
- O contrato concreto do endpoint externo permanece fora deste escopo; o Worker usa uma interface e um cliente HTTP configurado por `ExternalService:BaseUrl`.
- A estratégia de claim usa entidade rastreada pelo EF Core: `ClaimNextAsync` prepara `Status`, `LockId`, `LockedUntil` e `Attempts`, e a Application confirma com `IUnitOfWork.SaveChangesAsync` antes do HTTP. `Status`, `LockedUntil` e `LockId` são tokens de concorrência; uma `DbUpdateConcurrencyException` significa perda da reivindicação.
- Falhas de persistência que escapam do processor são capturadas no limite do `BackgroundService`, registradas e fazem o worker aguardar o próximo ciclo de 5 segundos; o cancelamento normal do host é encerrado sem log de erro de negócio.

## BDD Coverage Matrix - backend

| BDD ID | Cenário | Criticidade | Cobertura | Tipo | Evidência | Status | Justificativa |
|---|---|---:|---|---|---|---|---|
| FTH-005-BDD-001 | Serviço hospedado consulta a Outbox a cada 5 segundos | critical | inspeção estática do hosted service e registro | manual | `src/backend/FundoTakeHome.Api/BackgroundServices/OutboxBackgroundService.cs` usa `PeriodicTimer` de 5 segundos; `Program.cs` registra `AddHostedService<OutboxBackgroundService>()` | covered | O processamento roda em background dentro da API, fora da requisição HTTP. |
| FTH-005-BDD-002 | Claim muda `Pending` para `Processing` atomicamente e atribui `LockId`/`LockedUntil` | critical | inspeção da operação de claim e build | manual | `OutboxMessageStore.ClaimNextAsync` prepara a entidade rastreada; `Status`, `LockedUntil` e `LockId` são tokens de concorrência; `OutboxProcessor` confirma via `IUnitOfWork` antes do HTTP | covered | O `UPDATE` do EF inclui os valores originais dos tokens; conflito significa perda da reivindicação. |
| FTH-005-BDD-003 | Lease dura 30 segundos e mensagem expirada pode ser reivindicada novamente | critical | inspeção da persistência e do claim | manual | `OutboxProcessor` define lease de 30 segundos; `OutboxMessageStore` aceita `Processing` com `LockedUntil <= now` | covered | O comportamento está literal no código de Application/Infrastructure. |
| FTH-005-BDD-004 | Persistência do claim termina antes da chamada externa | critical | inspeção estática | manual | `OutboxProcessor` chama `IUnitOfWork.SaveChangesAsync` antes de `IApprovedApplicationIntegration` | covered | A Application confirma o claim antes do envio; o store não abre commit próprio. |
| FTH-005-BDD-005 | Customer e Application atuais são carregados pelos IDs do evento | critical | inspeção da store e teste do fluxo de Application | xUnit/manual | `OutboxMessageStore.LoadDeliveryAsync` filtra pelos IDs do envelope; `OutboxProcessorTests` valida o caminho de entrega usando a porta | execution_pending | A implementação foi inspecionada; não há teste executado contra persistência real. |
| FTH-005-BDD-006 | Operações `Created` e `Updated` chamam a integração com operação correta | critical | teste de Application | xUnit | `tests/FundoTakeHome.Tests/Features/ApprovedApplicationDelivery/OutboxProcessorTests.cs` | execution_pending | Os dois testes foram criados e aguardam execução humana. |
| FTH-005-BDD-007 | HTTP 200 marca `Processed` apenas com status e `LockId` ainda válidos | critical | inspeção da atualização concorrente | manual | `OutboxMessageStore.MarkProcessedAsync` exige `MessageId`, `Processing` e `LockId`; `Status`, `LockedUntil` e `LockId` participam do controle de concorrência do EF | covered | A conclusão stale retorna falso ou perde no `SaveChangesAsync`; nenhum desses casos é tratado como sucesso. |
| FTH-005-BDD-008 | Timeout, erro de rede e non-200 registram erro, incrementam tentativa e agendam 5 segundos | critical | teste de Application e inspeção da store | xUnit/manual | `OutboxProcessor` trata timeout/erro; `MarkFailedAsync` agenda `now.AddSeconds(5)` | execution_pending | Teste de falha criado; execução humana pendente. |
| FTH-005-BDD-009 | Terceira falha marca `Failed` e interrompe retries automáticos | critical | teste de Application e inspeção de status | xUnit/manual | `OutboxProcessorTests.ShouldMarkMessageAsFailedAfterThirdAttempt`; `MarkFailedAsync` usa `Failed` a partir da terceira tentativa | execution_pending | Teste criado; execução humana pendente. |
| FTH-005-BDD-010 | Customer/Application ausente marca a mensagem como `Failed` | critical | teste de Application | xUnit | `OutboxProcessorTests.ShouldMarkMessageAsFailedWhenReferencedDataIsMissing` | execution_pending | Teste criado; execução humana pendente. |
| FTH-005-BDD-011 | `Attempts`, `LastError`, `ProcessedAt`, `LockedUntil` e `LockId` são registrados corretamente | critical | inspeção da entidade, configuração e migration | manual | `OutboxMessage.cs`, `FundoTakeHomeDbContext.cs` e `InitialCreate.cs` | covered | Campos e mapeamentos estão explícitos. |
| FTH-005-BDD-012 | Evento inclui `eventId` e entrega é at-least-once | critical | inspeção do payload e fluxo | manual | `ApprovedApplicationEvent`, `OutboxEventEnvelope` e estados `Pending`/`Failed` | covered | O `eventId` é preservado e falhas liberam nova tentativa. |
| FTH-005-BDD-013 | Não são adicionados Polly, broker ou infraestrutura desnecessária | normal | inspeção estática dos arquivos alterados | manual | diff do Backend, Worker, Compose e referências NuGet | covered | Nenhuma dessas dependências foi adicionada. |

## Gates executados

- Gate A: PASS — estrutura aprovada; API única com processamento em background e testes limitados a Application/Domain.
- Micro-gate 1.1 API/Hosts: PASS — `BackgroundService` hospedado na API, polling de 5 segundos e DI configurados.
- Micro-gate 1.2 Application: PASS — processor, portas e DTOs isolam o caso de uso de HTTP e EF.
- Micro-gate 1.3 Domain: PASS — estados e regras de entrega estão representados sem alterar o fluxo de aprovação.
- Micro-gate 1.4 Testes: WARN — testes de Application criados e compiláveis; execução fica para o usuário.
- Micro-gate 1.5 Infrastructure: PASS — claim, lease, lock-safe completion, retries, migration e Compose configurados.
- Micro-gate 1.6 Architecture Gates: WARN — build e preflight do Compose passaram; suíte automatizada permanece pendente por regra da etapa.
- Fable consolidado: reservado para o fechamento da etapa.

## Classificação das evidências

- **Implementado:** processamento em background, claim com lease, tokens de concorrência, conclusão protegida por `LockId`, retry fixo, validação de falhas e integração atrás de porta.
- **Verificado por inspeção:** boundaries, ordem `claim → SaveChangesAsync → HTTP`, elegibilidade do lease, transições da Outbox, configuração/DI e ausência de infraestrutura extra.
- **Verificado por build:** compilação da solução com `dotnet build FundoTakeHome.slnx --no-restore`.
- **Não verificado por teste:** execução da suíte xUnit e cenários que dependem de persistência real/concorrência real.

## Evidência — checklist 1.1

- **Evidência:** `BackgroundService` registrado na API e configurado para polling periódico.
- **Arquivos:** `src/backend/FundoTakeHome.Api/Program.cs`, `BackgroundServices/OutboxBackgroundService.cs`, `FundoTakeHome.Api.csproj`.
- **Testes:** nenhum — host validado por build.
- **Micro-gate:** PASS.
- **O que o código prova agora:** a API inicia o processamento em background e cria escopos por processamento.
- **O que ainda não prova:** execução ponta a ponta contra o mock externo.

## Evidência — checklist 1.2

- **Evidência:** processor coordena claim, carga dos dados, integração, sucesso e falha por interfaces.
- **Arquivos:** `src/backend/FundoTakeHome.Api/Features/ApprovedApplicationDelivery/Application/**`.
- **Testes:** `OutboxProcessorTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** Application não depende diretamente de EF ou detalhes de endpoint.
- **O que ainda não prova:** execução da suíte pelo usuário.

## Evidência — checklist 1.3

- **Evidência:** estados da Outbox, tentativas, lease e operação do evento foram representados.
- **Arquivos:** `src/backend/FundoTakeHome.Api/Infrastructure/Persistence/Outbox/OutboxStatusEnum.cs`, `src/backend/FundoTakeHome.Api/Infrastructure/Persistence/Outbox/OutboxMessage.cs`, `src/backend/FundoTakeHome.Api/Features/ApprovedApplicationDelivery/Application/Models/OutboxEventEnvelope.cs`.
- **Testes:** testes de Application/Domain foram criados para as transições observáveis; a execução da suíte permanece pendente do usuário.
- **Micro-gate:** PASS.
- **O que o código prova agora:** mensagens podem avançar entre Pending, Processing, Processed e Failed segundo a política do card.
- **O que ainda não prova:** execução automatizada.

## Evidência — checklist 1.4

- **Evidência:** testes de Application cobrem sucesso, Created, Updated, retry, terceira falha e dados ausentes.
- **Arquivos:** `tests/FundoTakeHome.Tests/Features/ApprovedApplicationDelivery/OutboxProcessorTests.cs`.
- **Testes:** vinte e um testes de Application/Domain; execução pendente do usuário.
- **Micro-gate:** WARN.
- **O que o código prova agora:** há artefatos compiláveis para os cenários centrais do processor.
- **O que ainda não prova:** resultado da execução da suíte.

## Evidência — checklist 1.5

- **Evidência:** store SQLite prepara claim e conclusão com entidade rastreada; `Status`, `LockedUntil` e `LockId` são tokens de concorrência, enquanto a Application coordena cada `SaveChangesAsync`; Worker possui cliente HTTP configurável.
- **Arquivos:** `src/backend/FundoTakeHome.Api/Features/ApprovedApplicationDelivery/Infrastructure/Persistence/OutboxMessageStore.cs`, `src/backend/FundoTakeHome.Api/Infrastructure/Persistence/FundoTakeHomeDbContext.cs`, `src/backend/FundoTakeHome.Api/Infrastructure/Persistence/Migrations/InitialCreate.cs`, `src/backend/FundoTakeHome.Api/Features/ApprovedApplicationDelivery/Infrastructure/ExternalServices/ApprovedApplicationHttpClient.cs`, `docker-compose.yml`, API `Dockerfile`.
- **Testes:** nenhum teste próprio de Infrastructure.
- **Micro-gate:** PASS.
- **O que a inspeção estática indica:** o claim é confirmado antes do envio e mensagens stale não devem ser concluídas; perda de concorrência durante a confirmação é tratada como perda do lease. Falhas de banco são propagadas ao hosted service, que registra o erro e aguarda o próximo polling.
- **O que ainda não prova:** contrato HTTP específico do serviço externo.

## Evidência — checklist 1.6

- **Evidência:** `dotnet build FundoTakeHome.slnx --no-restore` e `docker compose config --quiet` passaram; revisão estática do diff concluída.
- **Arquivos:** solution, projetos, Worker, Backend, Compose, checklist e handoff.
- **Testes:** não executados pelo agente.
- **Micro-gate:** WARN.
- **O que o código prova agora:** todos os projetos compilam e a configuração do Compose é válida.
- **O que ainda não prova:** execução humana da suíte e integração ponta a ponta com o mock.

### Checklist arquitetural — Background Event Worker

- **Momento:** pré-código, pré-Infra e Gates
- **Mapa:** API = host e composição; `ApprovedApplicationDelivery` = Application; `OutboxMessageStore` e `ApprovedApplicationHttpClient` = Infrastructure; `OutboxMessage`/evento e DbContext = Persistence; `BackgroundService` apenas dispara o processamento fora da requisição.
- **Q:** OK
- **Varredura cross-UC:** busca feita por stores, mappers, loaders e referências de outros use cases; sem twin ou dependência indevida.
- **Resultado:** PASS

## Resultado do use case

1. Worker host e polling: PASS
2. Application e integração: PASS
3. Estados, lease e retries: PASS
4. Testes de Application e Domain: WARN
   Motivo: os testes foram criados e compilam, mas a execução fica para o usuário.
5. Infrastructure e persistência: PASS
6. Build e Compose: PASS
7. BDD Coverage Matrix: WARN
   Motivo: cenários com xUnit aguardam execução humana.
8. Checklist arquitetural: PASS

Status geral: PASS com execução de testes pendente.

## Pendências e riscos

- Executar a suíte xUnit manualmente.
- Configurar o endpoint HTTP concreto do serviço externo quando o contrato estiver disponível.

## Validação final — checklist 7

- **Arquivos revisados:** `Program.cs`, `OutboxBackgroundService.cs`, `OutboxProcessor.cs`, `OutboxMessageStore.cs`, `SubmitApplicationHandler.cs`, `IApprovedApplicationIntegration.cs`, `ApprovedApplicationHttpClient.cs`, README e documentos do FTH-005.
- **Verificações executadas:** `dotnet build FundoTakeHome.slnx --no-restore` passou; `git diff --check` passou; referências, DI, configuração e documentação foram inspecionadas.
- **Resultado:** polling de 5 segundos, lease de 30 segundos, commit do claim antes do HTTP, retry fixo, terceira falha permanente, dados atuais, operações `Created`/`Updated` e exclusão automática de `Failed` foram confirmados por inspeção.
- **Suíte automatizada:** não executada pelo agente, conforme regra da etapa.
- **FTH-006:** endpoint, payload HTTP detalhado e mock externo permanecem pendentes e fora do escopo do FTH-005.
