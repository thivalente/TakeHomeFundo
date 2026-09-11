# Checklist de execução — backend

Ticket: FTH-005

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
- Não alterar o fluxo de aprovação/persistência do FTH-002, salvo bloqueio real autorizado.
- Manter a API em `01- Presentation` e os testes em `02- Tests`; não criar projeto separado para o processamento em background.
- Não implementar endpoint ou contrato HTTP do mock; essa parte pertence ao FTH-006.
- Não usar Polly, broker, circuit breaker, dead-letter queue ou retry infrastructure adicional.
- Não rebaixar use case ou gate por evidência fora do escopo do item atual.

- [x] 1. Worker de processamento da Outbox de aplicações aprovadas
  - BDD IDs aplicáveis: FTH-005-BDD-001 a FTH-005-BDD-013; evidências previstas no handoff.
  - [x] 1.1 API
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** `BackgroundService` registrado dentro do projeto `FundoTakeHome.Api`, com escopo de dependência correto e sem chamada externa no request original.
  - [x] 1.2 Application
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** caso de uso/serviço coordena reivindicação, carregamento dos dados atuais, integração, sucesso e falha sem conter detalhes de HTTP ou persistência concreta.
  - [x] 1.3 Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** estados, lease, tentativas, backoff e regras de transição possuem representação coerente e não permitem conclusão por worker sem o `LockId` correto.
  - [x] 1.4 Testes de Application e Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; criar ou revisar os testes exigidos, mas não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** somente testes de Application e Domain, compiláveis, cobrem sucesso, retry, backoff, terceira falha, proteção de conclusão por `LockId` e operações `Created`/`Updated`; lease expirado/não expirado é validado por inspeção da regra do store, sem teste próprio de Infrastructure.
  - [x] 1.5 Infrastructure
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** claim protegido por tokens de concorrência do EF Core, lease de 30 segundos, commit antes do HTTP, persistência de todos os campos de controle, retry de 5 segundos e cliente de integração registrados; mensagens `Failed` não são revendidas automaticamente; validar por inspeção e build, sem criar testes de Infrastructure.
  - [x] 1.6 Backend Architecture Gates
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** análise arquitetural estática, build, matriz BDD e validações aplicáveis concluídos; resultados `PASS`/`WARN`/`FAIL` registrados no handoff, sem falha bloqueante.

## Registro de correções e evidências

- Item 1 — boundary da unidade de trabalho: concluído e verificado por inspeção.
- Item 2 — claim e conclusão concorrentes: concluído e verificado por inspeção; a suíte não foi executada.
- Item 3 — resiliência do hosted service: concluído e verificado por inspeção; a suíte não foi executada.
- Item 4 — testes de Application/Domain: testes criados e compilados; execução pendente do usuário.
- Pendências: execução da suíte xUnit e validação ponta a ponta com o mock do FTH-006.
