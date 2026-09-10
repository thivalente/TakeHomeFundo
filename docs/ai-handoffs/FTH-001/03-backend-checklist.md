# Checklist de execução — backend

Ticket: FTH-001

## Regras fixas obrigatórias

- SEGUIR as regras do workflow `new-feature`.
- Executar uma microentrega por autorização humana explícita.
- Seguir estritamente o escopo do item atual.
- Executar sempre o próximo item elegível; não pular itens pendentes.
- Verificar reuso antes de criar qualquer artefato novo.
- Aplicar e obedecer `.cursor/rules/backend-core.mdc` antes de criar ou alterar C#, inclusive records posicionais; não copiar essa regra neste checklist.
- Criar e revisar testes quando o subitem exigir, mas nunca executar xUnit ou outra suíte automatizada; a execução fica para o usuário.
- Não antecipar testes formais antes do subitem 1.4.
- Não antecipar Infrastructure, gates ou itens posteriores ao escopo autorizado.
- Não rebaixar use case ou gate por evidência fora do escopo do item atual.
- Preservar a solution existente `FundoTakeHome.slnx`.
- API em `src/backend/FundoTakeHome.Api`, dentro da solution folder `01- Api`.
- Testes em `tests/FundoTakeHome.Tests`, dentro da solution folder `02- Tests`.
- O objetivo mínimo demonstrável é API iniciando, endpoint de teste respondendo e Swagger acessível.
- Não implementar neste card o fluxo completo de submissão, aprovação, negação, worker ou comportamento do mock.

- [x] 1. Backend Foundation e endpoint de teste
  - BDD IDs aplicáveis: `FTH-001-BDD-001` a `FTH-001-BDD-009`; evidências previstas estão na BDD Coverage Matrix do handoff.
  - [x] 1.1 API
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** projeto Minimal API criado no caminho correto, registrado na solution folder `01- Api`, com endpoint de teste e Swagger configurado.
  - [x] 1.2 Application
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** estrutura Vertical Slice/Clean Architecture preparada para `SubmitApplication`, com abstrações iniciais de Strategy, ErrorOr e FluentValidation disponíveis, sem implementar o fluxo completo.
  - [x] 1.3 Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** fronteira de domínio e contrato base do rule engine definidos sem regras de aprovação ou negação deste card.
  - [x] 1.4 Testes de Application e Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; criar ou revisar os testes exigidos, mas não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** projeto xUnit criado no caminho correto, registrado na solution folder `02- Tests`, com Shouldly e `WebApplicationFactory` configurados e testes mínimos preparados.
  - [x] 1.5 Infrastructure
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** SQLite com EF Core e transações reais configurado; fundação do Outbox, Dockerfiles, Compose, volume SQLite, rede, portas e variáveis preparados; `src/frontend` preservado.
  - [x] 1.6 Backend Architecture Gates
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** análise arquitetural estática executada; `.cursor/rules/backend-architecture-gates.mdc` revisada; rules, skills e agents de análise backend aplicáveis executados; resultados `PASS`/`WARN`/`FAIL` registrados no handoff por check; sem rerodar suíte de testes nesta rodada.
