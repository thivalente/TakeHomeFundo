# Checklist de execução — backend

Ticket: FTH-006

## Regras fixas obrigatórias

- SEGUIR as regras do workflow new-feature.
- Executar uma microentrega por autorização humana.
- Seguir estritamente o escopo do item atual.
- Executar sempre o próximo item elegível, sem pular itens pendentes.
- Verificar reuso antes de criar artefato novo.
- Aplicar e obedecer `.cursor/rules/backend-core.mdc` antes de criar ou alterar C#, inclusive records posicionais; não copiar essa regra neste checklist.
- Criar e revisar testes quando o subitem exigir, mas nunca executar xUnit ou outra suíte automatizada; a execução fica para o usuário.
- Não antecipar testes formais antes do subitem 1.4.
- Não antecipar Infrastructure, gates ou itens posteriores.
- Não rebaixar use case ou gate por evidência fora do escopo do item atual.
- Não adicionar banco, cache, persistência, Outbox, regras de negócio, autenticação ou autorização.
- O mock deve permanecer stateless e não pode acessar o banco da aplicação principal.
- Não implementar alterações no worker; a seleção entre POST e PUT pertence ao FTH-005.
- O SSN completo pode ser recebido no payload, mas nunca pode aparecer nos logs.
- Testes automatizados estão fora do escopo deste card; a validação principal será manual.

- [~] 1. External Service Mock
  - BDD aplicável: não informado no card; registrar no handoff a justificativa de cobertura manual.
  - [x] 1.1 API
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Escopo:** criar o projeto .NET Minimal API na solução e expor `POST /customers` e `PUT /customers/{customerId}`.
    - **Critério de pronto:** os dois endpoints recebem o payload completo, inferem `Created`/`Updated`, retornam HTTP 200 com `{ "status": "received" }` e registram os dados exigidos, com SSN mascarado.
  - [ ] 1.2 Application
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A justificado no handoff caso o mock permaneça um projeto Minimal API sem camada Application separada e sem regra de aplicação própria.
  - [ ] 1.3 Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A justificado no handoff; o mock não possui regras de negócio, elegibilidade ou decisão de cliente novo/retornando.
  - [ ] 1.4 Testes de Application e Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; criar ou revisar os testes exigidos, mas não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A justificado no handoff, pois testes automatizados estão explicitamente fora do escopo do FTH-006; registrar o plano de verificação manual.
  - [ ] 1.5 Infrastructure
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A justificado no handoff; não há banco, cache, persistência, Outbox, acesso ao banco principal ou infraestrutura adicional além do wiring mínimo da API e do logger padrão.
  - [ ] 1.6 Backend Architecture Gates
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** análise arquitetural estática executada; `.cursor/rules/backend-architecture-gates.mdc` revisada; escopo confirmado como stateless; logs não expõem SSN; validação manual documentada; resultados `PASS`/`WARN`/`FAIL` registrados no handoff.

## Verificação manual prevista

- Iniciar o mock em terminal próprio.
- Enviar uma aplicação aprovada e confirmar log `POST /customers` com operação `Created` e resposta HTTP 200.
- Enviar novamente o mesmo SSN pelo fluxo existente e confirmar log `PUT /customers/{customerId}` com operação `Updated` e resposta HTTP 200.
- Confirmar que o SSN completo não aparece no terminal.
