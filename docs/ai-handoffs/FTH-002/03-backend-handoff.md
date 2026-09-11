# Backend Handoff — FTH-002

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_fechamento_final
- **Microentregas autorizadas:** —
- **Última aprovação humana:** autorização microentrega 1.1–1.6

## Planejamento global

### Use case: Approved Application

- **Objetivo:** registrar uma aplicação aprovada para cliente novo ou retornante sem duplicar dados.
- **Entrada:** request HTTP com nome, sobrenome, endereço, empresa, valor solicitado e SSN.
- **Saída:** envelope `{ data, errors }`; cliente novo retorna `201 Created` com `Location`, cliente retornante retorna `200 OK`.
- **Regras principais:** validar request; localizar cliente por SSN; criar ou atualizar Customer e Application; criar OutboxMessage; manter IDs existentes em atualização.
- **Dados trafegados/transformados:** request → Value Objects/entidades → persistência; resposta contém apenas IDs e status, sem SSN.
- **BC / jornada / passo:** Loan Application / submissão / fluxo aprovado.
- **Patterns obrigatórios:** Minimal API, Vertical Slice, Clean Architecture, FluentValidation, ErrorOr, Value Objects e transactional outbox.
- **Arquivos esperados:** `Features/SubmitApplication/**`, `Common/**`, `Infrastructure/Persistence/**`, testes unitários em `tests/FundoTakeHome.Tests/**` e documentação deste handoff/checklist.
- **Arquivos proibidos:** frontend, worker, Rule Engine/negação, serviço externo, autenticação, testes de integração e camadas genéricas sem necessidade.

### Escopo e ownership

- **Autenticação:** fora do escopo; não há usuário autenticado.
- **Ownership:** o SSN normalizado identifica o Customer e a Application retornante neste desafio.
- **Onde nasce:** vem na request e é normalizado pelo Value Object `Ssn`.
- **Contexto existente:** não há contexto autenticado; o lookup por SSN é necessário para decidir criação ou atualização.
- **Lookup adicional:** somente Customer por SSN, carregando sua Application; nenhum lookup de ownership adicional.

### Persistência

- Usar porta específica do use case, como `IApprovedApplicationStore`.
- Persistir Customer, Application e OutboxMessage com uma única chamada a `SaveChangesAsync()`.
- Garantir unicidade de `Customer.Ssn` e `Application.CustomerId`.
- Gerar IDs GUID/UUID v7 antes do save.
- Usar `ApplicationStatus.Approved`, `OutboxEventType.ApplicationApproved`, `EntityOperation.Created/Updated` e `OutboxStatus.Pending`.

## BDD Coverage Matrix - backend

| BDD ID | Cenário | Criticidade | Cobertura | Tipo | Evidência | Status | Justificativa |
|---|---|---:|---|---|---|---|---|
| FTH-002-BDD-001 | Endpoint aceita request aprovada válida | critical | inspeção manual do endpoint | manual | `POST /api/applications` com request válida retornou 201 | covered | Fluxo validado manualmente. |
| FTH-002-BDD-002 | Request inválida retorna todos os erros em uma única resposta 400 | critical | teste unitário do validator | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationRequestValidatorTests.cs::ShouldReturnAllErrors_WhenRequestIsInvalid` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-002-BDD-003 | Cliente novo cria Customer, Application e OutboxMessage | critical | teste unitário do handler | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationHandlerTests.cs::ShouldCreateCustomerApplicationAndEvent_WhenCustomerIsNew` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-002-BDD-004 | Cliente retornante atualiza Customer e Application sem duplicar | critical | teste unitário do handler + validação manual | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationHandlerTests.cs::ShouldUpdateCustomerAndApplicationWithoutChangingIds_WhenCustomerReturns` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-002-BDD-005 | IDs existentes são preservados na atualização | critical | teste unitário do handler + resposta manual | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationHandlerTests.cs::ShouldUpdateCustomerAndApplicationWithoutChangingIds_WhenCustomerReturns` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-002-BDD-006 | Customer.Ssn e Application.CustomerId são únicos | critical | configuração EF e índices únicos | manual | `src/backend/FundoTakeHome.Api/Infrastructure/Persistence/FundoTakeHomeDbContext.cs` — `HasIndex(...).IsUnique()` | covered | Índices conferidos e tabelas criadas no SQLite. |
| FTH-002-BDD-007 | Customer, Application e OutboxMessage usam um único SaveChangesAsync | critical | inspeção estática do handler e unit of work | manual | `src/backend/FundoTakeHome.Api/Features/SubmitApplication/Application/Handlers/SubmitApplicationHandler.cs::HandleAsync` | covered | Há uma chamada literal a `SaveChangesAsync` após os adds/updates. |
| FTH-002-BDD-008 | Novo cliente retorna 201 com Location e retornante retorna 200 | critical | validação manual do endpoint | manual | request nova retornou 201 + Location; request retornante retornou 200 | covered | Ambos os status foram observados. |
| FTH-002-BDD-009 | Respostas usam envelope padrão e não expõem SSN | critical | inspeção e resposta manual | manual | `SubmitApplicationEndpoints.cs`; respostas observadas com `data`/`errors` e sem SSN | covered | Envelope e minimização conferidos. |
| FTH-002-BDD-010 | Value Objects e invariantes de domínio rejeitam entradas inválidas sem exceção esperada | critical | testes unitários | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationDomainTests.cs` | execution_pending | Testes compiláveis; execução fica para o usuário. |
| FTH-002-BDD-011 | Outbox inicia com payload mínimo, Pending, Attempts 0 e timestamps corretos | critical | inspeção do store e do payload | manual | `Features/SubmitApplication/Infrastructure/Persistence/ApprovedApplicationStore.cs` e `Infrastructure/Persistence/Outbox/ApprovedApplicationEvent.cs` | covered | Valores iniciais e payload mínimo estão explícitos. |
| FTH-002-BDD-012 | Resposta de sucesso só ocorre após commit | critical | inspeção do fluxo | manual | `SubmitApplicationHandler.cs` aguarda `unitOfWork.SaveChangesAsync` antes de retornar sucesso | covered | Ordem do commit e resposta conferida no código. |

## Estado atual

- Gate A e execução dos itens 1.1–1.6 concluídos nesta autorização.
- Fechamento consolidado ainda aguarda aprovação humana explícita.
- Build da solution concluído sem erros.
- A execução da suíte xUnit permanece pendente para o usuário, conforme regra da etapa.

## Gates executados

- Gate A: PASS — planejamento, checklist e matriz BDD definidos antes do código.
- Micro-gate 1.1 API: PASS — endpoint, envelope, validação e status HTTP verificados manualmente.
- Micro-gate 1.2 Application: PASS — handler coordena lookup, criação/atualização, domínio e persistência.
- Micro-gate 1.3 Domain: PASS — entidades, Value Objects, enums, invariantes e GUID v7 implementados.
- Micro-gate 1.4 Testes: WARN — testes unitários criados e compiláveis; execução fica para o usuário.
- Micro-gate 1.5 Infrastructure: PASS — EF/SQLite, constraints, lookup com Application, outbox e um SaveChangesAsync.
- Micro-gate 1.6 Architecture Gates: WARN — build PASS; avisos xUnit1051 permanecem em testes existentes e novos; não são bloqueantes.

### Checklist arquitetural — Approved Application

- **Momento:** pré-código
- **Mapa:** `SubmitApplicationEndpoints` = API; `SubmitApplicationHandler`/`IApprovedApplicationStore` = Application; entidades e Value Objects = Domain; `ApprovedApplicationStore`/`FundoTakeHomeDbContext` = Infrastructure.
- **Q:** OK
- **Varredura cross-UC:** busca feita no repositório; sem twin ou dependência de outro use case.
- **Resultado:** PASS

### Checklist arquitetural — Approved Application / Infrastructure

- **Momento:** pré-Infra
- **Mapa:** `ApprovedApplicationStore` = Features/SubmitApplication/Infrastructure/Persistence; `OutboxMessage`/evento = Infrastructure/Persistence/Outbox compartilhada; consumidores: submissão e entrega da aplicação aprovada.
- **Q:** OK
- **Varredura cross-UC:** mappers/loaders/queries/stores equivalentes buscados; sem twin e sem referência a outro UC.
- **Resultado:** PASS

### Checklist arquitetural — Approved Application / Gates

- **Momento:** Gates
- **Mapa:** arquivos alterados conferidos contra as fronteiras API, Application, Domain e Infrastructure do slice.
- **Q:** OK
- **Varredura cross-UC:** busca final feita; sem twin, owner incorreto ou dependência cross-UC.
- **Resultado:** PASS

### Evidência — checklist 1.1

- **Evidência:** validação manual confirmou `POST /api/applications`, envelope, `201` + `Location`, `200` e `400`.
- **Arquivos:** `Features/SubmitApplication/Endpoints/SubmitApplicationEndpoints.cs`, `Program.cs`, `Application/SubmitApplicationRequestValidator.cs`.
- **Testes:** `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationRequestValidatorTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** a borda HTTP recebe, valida e converte o resultado do use case para respostas RESTful.
- **O que ainda não prova:** execução automatizada da suíte.

### Evidência — checklist 1.2

- **Evidência:** handler compilado e validação manual confirmou criação e atualização pelo mesmo SSN.
- **Arquivos:** `Application/SubmitApplicationHandler.cs`, `Application/IApprovedApplicationStore.cs`, `Application/SubmitApplicationResult.cs`.
- **Testes:** `SubmitApplicationHandlerTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** Application coordena o fluxo e preserva IDs no cliente retornante.
- **O que ainda não prova:** execução automatizada dos testes.

### Evidência — checklist 1.3

- **Evidência:** entidades, Value Objects, enums e resultado de domínio compilam e são usados pelo handler.
- **Arquivos:** `Features/SubmitApplication/Domain/**`.
- **Testes:** `SubmitApplicationHandlerTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** invariantes de SSN, valor, estado e identidade são representadas no domínio.
- **O que ainda não prova:** execução automatizada dos testes.

### Evidência — checklist 1.4

- **Evidência:** testes unitários de handler, validator e Value Objects foram criados e compilam.
- **Arquivos:** `tests/FundoTakeHome.Tests/Features/SubmitApplication/**`.
- **Testes:** `SubmitApplicationHandlerTests.cs`, `SubmitApplicationRequestValidatorTests.cs`.
- **Micro-gate:** WARN.
- **O que o código prova agora:** existem artefatos de teste para criação, atualização, validação e Value Objects.
- **O que ainda não prova:** resultado da execução pelo usuário.

### Evidência — checklist 1.5

- **Evidência:** SQLite criou as três tabelas e índices únicos; logs confirmaram lookup com join e uma operação de save por request.
- **Arquivos:** `Infrastructure/Persistence/FundoTakeHomeDbContext.cs`, `Features/SubmitApplication/Infrastructure/Persistence/ApprovedApplicationStore.cs`, `Infrastructure/Persistence/Outbox/OutboxMessage.cs`.
- **Testes:** validação manual do endpoint.
- **Micro-gate:** PASS.
- **O que o código prova agora:** Customer, Application e OutboxMessage são persistidos atomicamente e sem duplicidade por SSN/CustomerId.
- **O que ainda não prova:** concorrência avançada, que está fora do escopo.

### Evidência — checklist 1.6

- **Evidência:** build, `git diff --check`, preflight estático e validação manual concluídos.
- **Arquivos:** código do slice, checklist e handoff.
- **Testes:** não executados pelo agente.
- **Micro-gate:** WARN.
- **O que o código prova agora:** o use case compila, inicia e responde nos fluxos principal, retornante e inválido.
- **O que ainda não prova:** execução humana dos testes unitários.

## Resultado do use case

1. Build: PASS
2. API e envelope: PASS
3. Application/CQRS: PASS
4. Domain e Value Objects: PASS
5. Persistência/transação/outbox: PASS
6. Cliente novo e retornante: PASS
7. Testes por evidência estática: WARN
   Motivo: os testes compilam, mas a execução fica para o usuário.
8. Checklist arquitetural: PASS
9. Handoff/checklist: PASS

Status geral: PASS com warning não bloqueante de execução dos testes.

## Correções solicitadas pelo usuário

- `DomainResult` removido; domínio e Value Objects usam `ErrorOr`.
- Normalização de texto movida para `Common/StringExtensions.cs`.
- `CustomerId`, `ApplicationId` e `OutboxMessageId` separados em `Domain/ValueObjects/Identifiers/`.
- `RequestedAmount`, `Ssn` e `UsState` movidos para `Domain/ValueObjects/` e mantidos como Value Objects com `ErrorOr`.
- Build após as correções: PASS; suíte xUnit não executada.
- Erros centralizados por responsabilidade em `Domain/Common/Errors` e `Application/Common/Errors`; build após a alteração: PASS.
- Validator usa `NotEmpty()` para campos textuais.
- `IUnitOfWork` foi criado em Application; o handler registra Add/Update e chama um único `SaveChangesAsync` ao final.
- `OutboxMessage.Status` usa `OutboxStatusEnum.Pending`, sem string hardcoded.

## Organização arquitetural atualizada

- `Customer` e `LoanApplication` estão em `Domain/Entities/`.
- `ApprovedApplicationEvent` está em `Infrastructure/Persistence/Outbox/`.
- `IDateTimeProvider` está em `Application/Interfaces/`, pois é uma porta consumida pelo handler.
- `SystemDateTimeProvider` está em `Infrastructure/Time/`, como implementação concreta da porta.
- `SubmitApplicationDomainMarker` foi removido por não representar comportamento de domínio.
