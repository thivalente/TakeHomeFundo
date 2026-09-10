# Backend Handoff — FTH-003

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_fechamento_final
- **Microentregas autorizadas:** —
- **Última aprovação humana:** autorização microentrega 1.1–1.6

## Planejamento global

### Use case: Denied Application

- **Objetivo:** rejeitar uma aplicação elegível para negação antes de qualquer persistência de Customer ou Application.
- **Entrada:** request válida de `POST /api/applications`, após validação e normalização dos Value Objects.
- **Saída:** HTTP 422 com `{ data: null, errors: [...] }` quando houver negações; fluxo aprovado do FTH-002 quando não houver.
- **Regras principais:** executar todas as regras; agregar todos os erros; NY gera `application.denied.state_ny`; SSN na blacklist gera `application.denied.ssn_blacklisted`; uma request pode gerar ambos.
- **Dados trafegados/transformados:** estado normalizado e SSN normalizado entram nas regras; o SSN não sai em resposta, log ou evento.
- **BC / jornada / passo:** Loan Application / submissão / decisão de elegibilidade.
- **Patterns obrigatórios:** Strategy, `IDecisionRule`, `IEnumerable<IDecisionRule>`, `ErrorOr`, porta de lookup e DI.
- **Arquivos esperados:** extensão de `Features/SubmitApplication/**`, persistência SQLite/migration/seed da blacklist, testes unitários em `tests/FundoTakeHome.Tests/**` e estes artefatos de handoff.
- **Arquivos proibidos:** frontend, worker, serviço externo, autenticação, auditoria de negação, validação de request e testes de endpoint/integração.

### Escopo e ownership

- **Autenticação:** fora do escopo; não há usuário autenticado.
- **Ownership:** não aplicável à decisão; a regra usa estado e SSN da própria request.
- **Identificador de escopo:** SSN normalizado é apenas chave de consulta da blacklist, não deve ser exposto.
- **Onde nasce:** request; o Value Object `Ssn` fornece a forma normalizada antes do Rule Engine.
- **Lookup adicional:** somente blacklist por SSN normalizado. Customer/Application são proibidos antes do resultado de negação.
- **Falhas de infraestrutura:** erro de banco não vira negação; deve chegar ao tratamento global do FTH-004 e resultar em HTTP 500.

## BDD Coverage Matrix - backend

| BDD ID | Cenário | Criticidade | Cobertura | Tipo | Evidência | Status | Justificativa |
|---|---|---:|---|---|---|---|---|
| FTH-003-BDD-001 | Estado NY retorna 422 com `application.denied.state_ny` | critical | mapeamento HTTP + teste da regra | manual + xUnit | `src/backend/FundoTakeHome.Api/Features/SubmitApplication/Endpoints/SubmitApplicationEndpoints.cs` + `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs::ShouldReturnStateDenial_WhenApplicationIsFromNewYork` | covered | O endpoint usa 422 para códigos de negação e a regra prova o código literal. |
| FTH-003-BDD-002 | SSN da blacklist retorna 422 com `application.denied.ssn_blacklisted` | critical | mapeamento HTTP + teste da regra | manual + xUnit | `src/backend/FundoTakeHome.Api/Features/SubmitApplication/Endpoints/SubmitApplicationEndpoints.cs` + `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs::ShouldReturnSsnDenial_WhenSsnIsBlacklisted` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-003-BDD-003 | Request que atende ambas as regras retorna os dois erros | critical | teste do Rule Engine | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs::ShouldReturnAllDenials_WhenMultipleRulesMatch` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-003-BDD-004 | Request sem negação segue o fluxo aprovado do FTH-002 | critical | teste do engine + handler | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs::ShouldReturnNoErrors_WhenNoDecisionRuleDenies` e `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationHandlerTests.cs::ShouldCreateCustomerApplicationAndEvent_WhenCustomerIsNew` | execution_pending | Testes compiláveis; execução fica para o usuário. |
| FTH-003-BDD-005 | Todas as regras registradas são executadas sem depender da ordem | critical | teste do Rule Engine | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs::ShouldReturnAllDenials_WhenMultipleRulesMatch` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-003-BDD-006 | Negação não consulta Customer/Application nem cria OutboxMessage | critical | teste do handler | xUnit | `tests/FundoTakeHome.Tests/Features/SubmitApplication/SubmitApplicationHandlerTests.cs::ShouldReturnDenialAndAvoidCustomerApplicationAndOutboxPersistence` | execution_pending | Teste compilável; execução fica para o usuário. |
| FTH-003-BDD-007 | Blacklist SQLite é criada, possui índice único e é semeada com dez SSNs | critical | migration e configuração EF | manual | `src/backend/FundoTakeHome.Api/Infrastructure/Persistence/Migrations/20260910203439_AddBlacklist.cs` e `FundoTakeHomeDbContext.cs` | covered | Migration e seed contêm a tabela, chave única e os dez valores literais. |
| FTH-003-BDD-008 | SSN nunca é exposto na resposta ou nos erros | critical | inspeção de regra e envelope | manual | `StateIsNyRule.cs`, `BlacklistedSsnRule.cs` e `SubmitApplicationEndpoints.cs` | covered | Mensagem, metadata e resposta usam somente o nome do campo; o valor do SSN não é incluído. |
| FTH-003-BDD-009 | Falha no lookup da blacklist propaga como erro inesperado 500 | critical | inspeção da porta, adapter e middleware | manual | `SqliteBlacklistSsnReader.cs`, `SubmitApplicationHandler.cs` e `GlobalExceptionHandlingMiddleware.cs` | covered | O adapter não captura exceção e o middleware global converte exceção inesperada em 500. |
| FTH-003-BDD-010 | Estratégias são testáveis isoladamente e registradas por DI | critical | inspeção de classes e DI | manual | `Application/DecisionRules/**`, `IBlacklistSsnReader.cs` e `Program.cs` | covered | Cada estratégia tem dependência explícita e ambas são registradas por `IEnumerable` no DI. |

## Estado atual

- Gate A aprovado antes da implementação.
- Itens 1.1–1.6 concluídos na autorização do use case completo.
- Suíte xUnit não executada pelo agente; execução fica para o usuário.
- Handoffs anteriores do repositório não foram alterados.

## Gates executados

- Gate A: PASS — planejamento e checklist aprovados antes do código.
- Micro-gate 1.1 API: PASS — endpoint mapeia erros de negação para 422 e preserva 400 para erros não-negociais.
- Micro-gate 1.2 Application: PASS — Rule Engine executa todas as estratégias e interrompe o fluxo antes do lookup de Customer.
- Micro-gate 1.3 Domain: PASS — regras de NY e blacklist possuem erros estáveis e não expõem o SSN.
- Micro-gate 1.4 Testes: WARN — testes de regras, engine e handler foram criados e compilam; execução fica para o usuário.
- Micro-gate 1.5 Infrastructure: PASS — entidade, chave única, seed, lookup, migration e DI foram criados.
- Micro-gate 1.6 Architecture Gates: PASS — build sem erros/avisos e diff sem falhas de whitespace; sem FAIL bloqueante identificado por inspeção estática.

### Checklist arquitetural — Denied Application

- **Momento:** pré-código
- **Mapa:** API = `SubmitApplicationEndpoints`; Application = handler, engine, regras e portas; Infrastructure = leitor SQLite, entidade, DbContext e migration; Domain = Value Objects existentes.
- **Q:** OK
- **Varredura cross-UC:** busca feita no slice do FTH-002; reuso de request, Value Objects, envelope, DbContext e DI; sem twin.
- **Resultado:** PASS

### Checklist arquitetural — Denied Application / Infrastructure

- **Momento:** pré-Infra
- **Mapa:** `SqliteBlacklistSsnReader` e `BlacklistedSsnRecord` pertencem a `Infrastructure/Persistence/Blacklist`; `IBlacklistSsnReader` permanece como porta em Application.
- **Q:** OK
- **Varredura cross-UC:** mappers/loaders/queries/stores equivalentes buscados; blacklist não duplica o store aprovado e não referencia outro UC.
- **Resultado:** PASS

### Checklist arquitetural — Denied Application / Gates

- **Momento:** Gates
- **Mapa:** fronteiras API, Application, Domain e Infrastructure conferidas contra o checklist; nenhum arquivo fora do escopo do use case.
- **Q:** OK
- **Varredura cross-UC:** busca final feita; sem twin, owner incorreto ou dependência indevida entre use cases.
- **Resultado:** PASS

### Evidência — checklist 1.1

- **Evidência:** endpoint retorna 422 para códigos `application.denied.*`, inclui o campo via metadata e preserva 400 para demais erros.
- **Arquivos:** `Features/SubmitApplication/Endpoints/SubmitApplicationEndpoints.cs`.
- **Testes:** não aplicável; endpoint/integration tests estão fora do escopo.
- **Micro-gate:** PASS.
- **O que o código prova agora:** negações são convertidas para o envelope padrão com HTTP 422.
- **O que ainda não prova:** validação manual por HTTP, reservada ao usuário.

### Evidência — checklist 1.2

- **Evidência:** `DecisionRuleEngine` percorre todos os `IDecisionRule<DecisionRuleInput>` e agrega todos os erros antes de permitir persistência aprovada.
- **Arquivos:** `Application/DecisionRuleEngine.cs`, `Application/DecisionRuleInput.cs`, `Application/SubmitApplicationHandler.cs`, `Application/Interfaces/IDecisionRule.cs`.
- **Testes:** `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** ausência de negação mantém o fluxo do FTH-002; negação impede lookup de Customer e save.
- **O que ainda não prova:** execução automatizada dos testes.

### Evidência — checklist 1.3

- **Evidência:** `StateIsNyRule` e `BlacklistedSsnRule` retornam os códigos e mensagens do card, com metadata de campo e sem o valor do SSN.
- **Arquivos:** `Application/DecisionRules/StateIsNyRule.cs`, `Application/DecisionRules/BlacklistedSsnRule.cs`, `Application/Interfaces/IBlacklistSsnReader.cs`.
- **Testes:** `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** ambas as regras são independentes e testáveis por porta.
- **O que ainda não prova:** execução automatizada dos testes.

### Evidência — checklist 1.4

- **Evidência:** testes compiláveis cobrem NY, SSN blacklist, engine com múltiplas negações e handler sem Customer/Application/Outbox em caso negado.
- **Arquivos:** `tests/FundoTakeHome.Tests/Features/SubmitApplication/DecisionRuleTests.cs`, `SubmitApplicationHandlerTests.cs`.
- **Testes:** os próprios arquivos acima; execução não realizada pelo agente.
- **Micro-gate:** WARN.
- **O que o código prova agora:** há evidência estática e testes unitários para as regras e o bloqueio de persistência.
- **O que ainda não prova:** resultado da execução pelo usuário.

### Evidência — checklist 1.5

- **Evidência:** migration cria a tabela `BlacklistedSsns`, usa o SSN normalizado como chave única, insere os dez valores e o DI registra leitor e estratégias; o lookup propaga exceções.
- **Arquivos:** `Infrastructure/Persistence/Blacklist/**`, `Infrastructure/Persistence/FundoTakeHomeDbContext.cs`, `Infrastructure/Persistence/Migrations/InitialCreate.cs`, `Program.cs`, `FundoTakeHome.Api.csproj`.
- **Testes:** validação estática da migration e do mapeamento; nenhum teste de integração.
- **Micro-gate:** PASS.
- **O que o código prova agora:** o setup local aplica a migration e disponibiliza a porta de blacklist.
- **O que ainda não prova:** aplicação da migration em uma instância SQLite pelo usuário.

### Evidência — checklist 1.6

- **Evidência:** `dotnet build FundoTakeHome.slnx --no-restore` passou sem erros ou avisos; `git diff --check` não encontrou falhas.
- **Arquivos:** checklist, handoff e código do use case.
- **Testes:** não executados pelo agente.
- **Micro-gate:** PASS.
- **O que o código prova agora:** o use case completo compila e os gates estáticos não apontaram bloqueio.
- **O que ainda não prova:** execução da suíte xUnit e validação HTTP manual.

## Resultado do use case

1. Build: PASS
2. Rule Engine/CQRS: PASS
3. Domain e regras de negação: PASS
4. API e envelope 422: PASS
5. Persistência SQLite/seed: PASS
6. Bloqueio de persistência em negação: PASS
7. Testes por evidência estática: WARN
   Motivo: os testes foram criados e compilam, mas a execução fica para o usuário.
8. Checklist arquitetural: PASS
9. Handoff/checklist: PASS

Status geral: PASS com warning não bloqueante de execução dos testes.
