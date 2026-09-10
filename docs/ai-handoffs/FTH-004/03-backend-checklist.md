# Checklist de execução — backend

Ticket: FTH-004

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
- Não alterar o envelope existente; neste card, criar o envelope mínimo necessário porque ele ainda não existe.
- Registrar a exceção real com `ILogger` no console, sem enviar seus detalhes na resposta HTTP.
- Não alterar o tratamento de validação ou erros de negócio.
- Não implementar logging personalizado, streaming ou testes de middleware neste card.

- [ ] 1. Tratamento global de erro inesperado da API
  - BDD IDs aplicáveis: cenários do FTH-004; evidências previstas no handoff.
  - [ ] 1.1 API
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** middleware global registrado na borda da API e exceções não tratadas encaminhadas ao tratamento comum.
  - [ ] 1.2 Application
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A — o card não altera Application.
  - [ ] 1.3 Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A — o card não altera Domain.
  - [ ] 1.4 Testes de Application e Domain
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; criar ou revisar os testes exigidos, mas não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A — testes do middleware estão fora do escopo do card.
  - [ ] 1.5 Infrastructure
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** N/A — não há alteração de persistência ou infraestrutura de dados.
  - [ ] 1.6 Backend Architecture Gates
    - Antes de executar, reler as regras fixas obrigatórias deste checklist.
    - Executar somente este subitem, seguindo as rules e skills aplicáveis; não executar Vitest, xUnit, Playwright ou qualquer suíte automatizada.
    - **Critério de pronto:** build e análise estática concluídos; resposta 500, envelope, mensagem pública, log real e preservação dos demais erros registrados no handoff.
