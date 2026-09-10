# Checklist de execução — backend

Ticket: FTH-003

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
- Não alterar o fluxo aprovado do FTH-002 além da integração necessária do Rule Engine.
- Não alterar validação de request, erros de sistema do FTH-004, worker, serviço externo, frontend, autenticação, auditoria de negação ou testes de endpoint/integração.
- Não expor o SSN em respostas, logs, erros ou eventos.
- Executar todas as regras registradas; a ordem das regras não faz parte do contrato.
- Em caso de negação, não consultar Customer/Application nem criar ou persistir dados de aplicação.
- Não rebaixar use case/gate por evidência fora do escopo do item atual.

- [x] 1. Denied Application
  - BDD IDs aplicáveis: FTH-003-BDD-001 a FTH-003-BDD-010; evidências detalhadas no handoff.
  - [x] 1.1 API
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** `POST /api/applications` converte a negação do handler para HTTP 422 no envelope padrão, sem quebrar o fluxo aprovado.
  - [x] 1.2 Application
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** handler normaliza os Value Objects, executa todas as `IDecisionRule` via `IEnumerable<IDecisionRule>`, agrega negações e só chama o fluxo do FTH-002 quando não há negação.
  - [x] 1.3 Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** `StateIsNyRule`, `BlacklistedSsnRule` e erros de negação representam as regras sem expor o SSN e permanecem testáveis por portas.
  - [x] 1.4 Testes de Application e Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; criar ou revisar os testes exigidos, mas não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** testes compiláveis cobrem as duas regras, zero/uma/múltiplas negações, handler negado e prova de que Customer/Application não são consultados nem OutboxMessage criado.
  - [x] 1.5 Infrastructure
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** SQLite cria e popula a blacklist com os dez SSNs repetidos, possui unicidade para SSN normalizado, expõe uma porta de lookup e registra as regras no DI; falhas de banco propagam como erro inesperado.
  - [x] 1.6 Backend Architecture Gates
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** build, análise estática, preflight BDD, revisão das regras arquiteturais e evidências do use case concluídos, sem FAIL bloqueante.
