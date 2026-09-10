# Checklist de execução — backend

Ticket: FTH-002

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
- Não implementar Rule Engine, negação, worker, serviço externo, frontend, autenticação ou testes de integração neste card.
- Não expor SSN em logs ou respostas.
- Não rebaixar use case/gate por evidência fora do escopo do item atual.

- [x] 1. Approved Application
  - BDD IDs aplicáveis: FTH-002-BDD-001 a FTH-002-BDD-012; evidências detalhadas no handoff.
  - [x] 1.1 API
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** `POST /api/applications` recebe a request, valida com FluentValidation, chama o handler e retorna o envelope padrão com status HTTP RESTful.
  - [x] 1.2 Application
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** handler transforma a request em objetos de domínio, consulta o cliente por SSN, cria ou atualiza o fluxo e converte falhas esperadas para `ErrorOr`.
  - [x] 1.3 Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** entidades, Value Objects, enums e regras do fluxo aprovado validam invariantes sem exceções para erros esperados; IDs são GUID/UUID v7.
  - [x] 1.4 Testes de Application e Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; criar ou revisar os testes exigidos, mas não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** testes unitários compiláveis cobrem Customer, Application, `Ssn`, `RequestedAmount`, `UsState`, OutboxMessage quando houver comportamento e o handler.
  - [x] 1.5 Infrastructure
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** `IApprovedApplicationStore` persiste Customer, Application e OutboxMessage atomically, com constraints únicas e uma única chamada a `SaveChangesAsync()`; novos retornam 201 + Location e retornantes 200.
  - [x] 1.6 Backend Architecture Gates
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** análise estática, build, preflight BDD, `.cursor/rules/backend-architecture-gates.mdc` e evidências do use case concluídos, sem FAIL bloqueante.
