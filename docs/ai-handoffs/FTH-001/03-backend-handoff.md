# Backend Handoff — FTH-001

## Controle da etapa

- **Autorização corrente:** aguardando_aprovacao_fechamento_final
- **Microentregas autorizadas:** —
- **Última aprovação humana:** autorização microentrega 1.1–1.6

## Planejamento global

### Objetivo

Preparar a fundação backend do TakeHome usando a solution existente `FundoTakeHome.slnx`, com API e testes organizados nas solution folders solicitadas, e entregar um ambiente mínimo demonstrável com endpoint de teste e Swagger.

### Use case / unidade

- **UC:** Backend Foundation e endpoint de teste.
- **Entrada:** requisição HTTP no endpoint de teste e configuração local do ambiente.
- **Saída:** resposta HTTP simples, Swagger acessível e base pronta para os próximos cards.
- **Regras principais:** manter o escopo do FTH-001; não implementar o fluxo completo de submissão nem regras de decisão.
- **Dados trafegados/transformados:** somente dados mínimos do endpoint de teste; nenhuma informação de cliente deve ser criada neste card.
- **BC / jornada / passo:** fundação técnica do desafio / ambiente backend / endpoint de teste. Não há BC de negócio aplicável antes do fluxo de submissão.
- **Patterns obrigatórios:** Minimal API, Vertical Slice leve, Clean Architecture por pastas, Strategy, ErrorOr, FluentValidation, EF Core/SQLite e Outbox foundation.
- **Arquivos esperados:** `FundoTakeHome.slnx`, `src/backend/FundoTakeHome.Api/**`, `tests/FundoTakeHome.Tests/**`, `docker-compose*`, Dockerfiles, README e este handoff/checklist.
- **Arquivos proibidos:** projetos de produção adicionais sem necessidade clara; implementação completa de submissão, aprovação, negação, worker ou mock.

### Organização obrigatória

- Solution: `D:\Projetos\Pessoal\Challenges\FundoTakeHome\FundoTakeHome.slnx`
- API: `src/backend/FundoTakeHome.Api`, solution folder `01- Api`.
- Testes: `tests/FundoTakeHome.Tests`, solution folder `02- Tests`.
- Frontend: manter `src/frontend` disponível para o próximo estágio.

### Escopo autenticado e ownership

- Autenticação não é necessária neste desafio.
- Não existe identificador de ownership neste card.
- Não propor lookup adicional de ownership.

### Decisões de persistência

- **Fluxo / capacidade:** fundação de persistência para os próximos use cases.
- **Tipo de acesso ao dado:** preparação de leitura + escrita futura; sem fluxo de negócio persistente neste card.
- **Contratos planejados ou alterados:** DbContext e fundação de Outbox, somente quando necessários.
- **Papel semântico de cada contrato:** persistência técnica e eventos pendentes.
- **Se misto: justificativa:** não aplicar contrato misto de negócio neste card.
- **Coexistência com outros contratos no fluxo:** não aplicável.

## BDD Coverage Matrix - backend

| BDD ID | Cenário | Criticidade | Cobertura | Tipo | Evidência | Status | Justificativa |
|---|---|---:|---|---|---|---|---|
| FTH-001-BDD-001 | A solution existente contém a API no caminho e solution folder definidos | critical | Inspeção da solution e dos paths | manual | `FundoTakeHome.slnx` contém `/01- Api/` e o projeto API | covered | Confirmado no arquivo da solution. |
| FTH-001-BDD-002 | A solution existente contém o projeto de testes no caminho e solution folder definidos | critical | Inspeção da solution e dos paths | manual | `FundoTakeHome.slnx` contém `/02- Tests/` e o projeto de testes | covered | Confirmado no arquivo da solution. |
| FTH-001-BDD-003 | A API inicia e o endpoint de teste responde | critical | Inspeção manual do endpoint | manual | `GET http://localhost:5080/api/health` retornou `200` e `{"status":"ok"}` | covered | Verificação manual concluída. |
| FTH-001-BDD-004 | O Swagger fica acessível | critical | Inspeção manual da página Swagger | manual | `GET http://localhost:5080/swagger/index.html` retornou `200` e contém `Swagger UI` | covered | Verificação manual concluída. |
| FTH-001-BDD-005 | SQLite e EF Core ficam configurados com transação real | critical | Inspeção estática de configuração e provider | manual | `FundoTakeHomeDbContext`, `UseSqlite` e `Microsoft.EntityFrameworkCore.Sqlite` | covered | Provider relacional SQLite configurado. |
| FTH-001-BDD-006 | O projeto de testes possui xUnit, Shouldly e WebApplicationFactory | critical | Projeto compila e referências são inspecionadas | xUnit | `tests/FundoTakeHome.Tests/ApiSmokeTests.cs` e referências do `.csproj` | execution_pending | Artefatos compilam; a execução da suíte fica para o usuário. |
| FTH-001-BDD-007 | Strategy, FluentValidation, ErrorOr e estrutura SubmitApplication estão disponíveis | normal | Inspeção estática de referências e pastas | manual | `Features/SubmitApplication/**`, referências NuGet e registro de validators | covered | Base disponível sem implementar o fluxo. |
| FTH-001-BDD-008 | A fundação do Outbox está preparada sem worker | normal | Inspeção estática de modelo/configuração | manual | `Infrastructure/Persistence/Outbox/OutboxMessage.cs` e `FundoTakeHomeDbContext.cs` | covered | Modelo e DbSet preparados; worker não foi antecipado. |
| FTH-001-BDD-009 | Docker Compose define serviços, volume SQLite, rede e configuração sem host hardcoded | critical | Build e execução real do Compose | manual | `docker-compose.yml`, Dockerfiles, `.dockerignore`; `docker compose build` e `docker compose config --quiet` passaram; Compose subiu e `GET http://localhost:8080/api/health` retornou `200` | covered | Serviços, rede, portas, volume e execução foram validados. |

## Estado inicial

- A solution existe, mas está vazia (`<Solution />`).
- Não há projetos de aplicação ou testes cadastrados.
- O SDK .NET disponível é `10.0.400`.
- Não há código de produto no repositório.
- Alteração existente do usuário em `DEVELOPMENT_LOG.md` foi preservada.

## Gates executados

- Gate A: PASS — checklist aprovado pelo usuário.
- Micro-gate 1.1 API: PASS — solution folder, endpoint e Swagger configurados.
- Micro-gate 1.2 Application: PASS — estrutura SubmitApplication e dependências disponíveis.
- Micro-gate 1.3 Domain: PASS — fronteira de domínio e Strategy base preparadas.
- Micro-gate 1.4 Testes: PASS — projeto compila com xUnit, Shouldly e WebApplicationFactory; execução pendente do usuário.
- Micro-gate 1.5 Infrastructure: PASS — SQLite, EF Core, Outbox, Compose e volume configurados; imagens Docker construídas e Compose executado com sucesso.
- Micro-gate 1.6 Architecture Gates: WARN — build passou, mas o restore reportou vulnerabilidades NU1903 transitivas em `Microsoft.OpenApi` e `SQLitePCLRaw.lib.e_sqlite3`; não há erro bloqueante.

## Evidências — checklist 1.1

- **Evidência:** solution e API foram criadas e a validação manual confirmou o endpoint e o Swagger.
- **Arquivos:** `FundoTakeHome.slnx`, `src/backend/FundoTakeHome.Api/Program.cs`, `src/backend/FundoTakeHome.Api/FundoTakeHome.Api.csproj`.
- **Testes:** `tests/FundoTakeHome.Tests/ApiSmokeTests.cs`.
- **Micro-gate:** PASS.
- **O que o código prova agora:** a API inicia, responde no endpoint de saúde e expõe Swagger.
- **O que ainda não prova:** o fluxo completo de submissão.

## Evidências — checklist 1.2

- **Evidência:** estrutura Vertical Slice preparada e pacotes ErrorOr/FluentValidation registrados.
- **Arquivos:** `src/backend/FundoTakeHome.Api/Features/SubmitApplication/**`, `Program.cs`.
- **Testes:** nenhum teste de fluxo de aplicação foi exigido neste card.
- **Micro-gate:** PASS.
- **O que o código prova agora:** a base do próximo slice pode ser implementada sem reestruturar a solution.
- **O que ainda não prova:** regras e handler de submissão.

## Evidências — checklist 1.3

- **Evidência:** fronteira Domain criada e contrato Strategy disponível na Application.
- **Arquivos:** `Features/SubmitApplication/Domain/SubmitApplicationDomainMarker.cs`, `Features/SubmitApplication/Application/IDecisionRule.cs`.
- **Testes:** nenhum teste de regra de negócio foi exigido neste card.
- **Micro-gate:** PASS.
- **O que o código prova agora:** regras futuras terão ponto de extensão independente.
- **O que ainda não prova:** aprovação ou negação de aplicações.

## Evidências — checklist 1.4

- **Evidência:** projeto de testes compilou e contém testes preparados para endpoint e Swagger.
- **Arquivos:** `tests/FundoTakeHome.Tests/FundoTakeHome.Tests.csproj`, `tests/FundoTakeHome.Tests/ApiSmokeTests.cs`.
- **Testes:** `ApiSmokeTests` — execução não realizada pelo agente.
- **Micro-gate:** PASS.
- **O que o código prova agora:** xUnit, Shouldly e WebApplicationFactory estão disponíveis e compiláveis.
- **O que ainda não prova:** resultado da execução da suíte pelo usuário.

## Evidências — checklist 1.5

- **Evidência:** build da solution passou; `docker compose build` e `docker compose config --quiet` passaram; o Compose subiu os três serviços e a API respondeu pelo container.
- **Arquivos:** `Infrastructure/Persistence/**`, `src/backend/FundoTakeHome.Api/Dockerfile`, `src/frontend/Dockerfile`, `src/mock/Dockerfile`, `.dockerignore`, `docker-compose.yml`, `README.md`.
- **Testes:** nenhum teste automatizado executado.
- **Micro-gate:** PASS.
- **O que o código prova agora:** SQLite, volume, rede, serviços e comandos básicos estão configurados e o ambiente sobe com os três containers.
- **O que ainda não prova:** comportamento do frontend e do mock, que estão fora do escopo.

## Evidências — checklist 1.6

- **Evidência:** análise estática, `dotnet build FundoTakeHome.slnx --no-restore`, `git diff --check` e preflight do Compose concluídos.
- **Arquivos:** solution, projetos, checklist, handoff e arquivos alterados do backend.
- **Testes:** não executados por regra da etapa.
- **Micro-gate:** WARN.
- **O que o código prova agora:** não há erro de compilação nem falha estrutural bloqueante identificada.
- **O que ainda não prova:** ausência de vulnerabilidades transitivas; há avisos NU1903 registrados.

## Configuração de desenvolvimento

- `src/backend/FundoTakeHome.Api/Properties/launchSettings.json` foi alinhado ao padrão de perfis nomeados do MotionHub.
- Perfil `Dev`: ambiente `Development`, porta `5080` e abertura automática do `/swagger`.
- Perfil `E2E`: ambiente `E2E`, porta `5081` e sem abertura automática do navegador.
- Validação do perfil `Dev`: endpoint de saúde retornou `200` e Swagger retornou `200`.

### Checklist arquitetural — Backend Foundation

- **Momento:** pré-código, pré-Infra e Gates.
- **Mapa:** API `Program.cs` — owner: Presentation — consumidor: endpoint de saúde — local: raiz do host — wiring HTTP mínimo; `FundoTakeHomeDbContext` — owner: Infrastructure/Persistence — consumidor: API/EF Core — local: Persistence — contexto técnico; `IDecisionRule` — owner: SubmitApplication/Application — consumidor: futuros handlers de decisão — local: slice — Strategy base do slice; `OutboxMessage` — owner: Infrastructure/Persistence/Outbox — consumidor: futuro worker — local: capacidade técnica — fundação sem worker.
- **Q:** OK.
- **Varredura cross-UC:** busca feita no repositório; não existem outros use cases implementados e não foi encontrado twin.
- **Resultado:** PASS.

## Resultado do use case

1. Build: PASS
2. API e endpoint de teste: PASS
3. Swagger: PASS
4. SQLite/EF Core: PASS
5. Strategy, FluentValidation e ErrorOr: PASS
6. Testes preparados: WARN
   Motivo: os artefatos compilam, mas a execução da suíte fica para o usuário.
7. Docker Compose e Dockerfiles: PASS
8. Checklist arquitetural: PASS
9. Handoff/checklist: PASS

Status geral: PASS com warning não bloqueante de execução dos testes e vulnerabilidades transitivas NU1903.
