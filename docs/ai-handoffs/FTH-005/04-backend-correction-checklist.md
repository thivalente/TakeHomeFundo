# Checklist de correção — FTH-005

## Objetivo

Este arquivo orienta a correção da entrega do FTH-005. Outro agente deve executar os itens na ordem indicada. O agente executor não deve considerar um item concluído apenas porque o código compila: cada item precisa cumprir os critérios de aceite e registrar evidência objetiva.

O escopo continua limitado ao FTH-005. Não implementar o mock externo nem fechar o contrato HTTP específico do FTH-006.

## Convenções

- `[ ]` pendente
- `[~]` em andamento
- `[x]` implementação concluída e validada por inspeção/build; quando houver teste automatizado, a execução precisa estar declarada separadamente

## Regras obrigatórias

- Manter `SaveChangesAsync` e a unidade de trabalho sob coordenação da camada Application.
- Não mover persistência, commits ou transações para dentro de uma camada que viole os boundaries atuais.
- Não adicionar Polly, broker, circuit breaker, dead-letter queue ou infraestrutura extra de retry.
- Não executar Vitest, Playwright, xUnit ou outra suíte automatizada durante a implementação desta etapa, conforme o checklist original do FTH-005.
- Pode executar build, `git diff --check`, análise estática e verificações explicitamente permitidas pelas rules.
- Preservar as alterações existentes fora do escopo, especialmente frontend, mock externo e contrato do FTH-006.

## Checklist ordenado

### 1. Responsabilidade da unidade de trabalho — concluído

- [x] **1.1 Confirmar o boundary correto.** A Application coordena o caso de uso e chama `IUnitOfWork.SaveChangesAsync`; a Infrastructure apenas consulta/prepara alterações por meio do `DbContext`.
  - [x] Não mover `SaveChangesAsync` para `OutboxMessageStore`.
  - [x] Não criar um commit independente dentro de `ClaimNextAsync`, `MarkProcessedAsync` ou `MarkFailedAsync` sem justificar impacto nos boundaries.
  - [x] Usar esta decisão como restrição para todos os itens seguintes.

**Critério de aceite:** a correção preserva a Application como dona da confirmação da unidade de trabalho e não introduz commit escondido na Infrastructure.

### 2. Claim e conclusão protegidos por concorrência — concluído

- [x] **2.1 Revisar `ClaimNextAsync` com base no comportamento real do EF Core.** O objetivo é garantir que duas execuções concorrentes não consigam confirmar a mesma mensagem com leases diferentes.
  - [x] Confirmar quais propriedades são tokens de concorrência e quais valores originais entram no `UPDATE` gerado pelo `SaveChangesAsync`.
  - [x] Garantir que uma reivindicação elegível altera `Status`, `LockId`, `LockedUntil` e `Attempts` antes da chamada HTTP.
  - [x] Manter a solução baseada em entidade rastreada e tokens de concorrência, documentar essa decisão e tratar `DbUpdateConcurrencyException` como perda da reivindicação.

- [x] **2.2 Revisar `MarkProcessedAsync` e `MarkFailedAsync`.** Uma execução que perdeu o lease ou possui `LockId` antigo não pode concluir nem liberar a mensagem de outra execução.
  - [x] A condição efetiva de persistência exige `MessageId`, `Status == Processing` e o mesmo `LockId`.
  - [x] Atualização concorrente durante `SaveChangesAsync` é tratada como perda do lease.
  - [x] O processor não trata uma atualização stale como sucesso.
  - [x] O fluxo Application → store → `SaveChangesAsync` foi preservado.

- [x] **2.3 Documentar o modelo escolhido.** O handoff registra que a proteção usa tokens de concorrência do EF Core, sem afirmar APIs inexistentes no código.

**Critério de aceite:** o código e a documentação demonstram o mesmo mecanismo; não há conclusão válida para um `LockId` antigo; `SaveChangesAsync` continua na Application.

### 3. Resiliência do `BackgroundService` — concluído

- [x] **3.1 Impedir que uma falha transitória encerre o worker.** Se o SQLite falhar durante claim, carregamento ou persistência, o processo deve registrar o erro e continuar disponível para o próximo ciclo.
  - [x] Tratamento adicionado no limite do loop do hosted service; o ciclo atual é interrompido e o próximo polling é aguardado.
  - [x] O cancelamento normal do host retorna sem ser registrado como erro de negócio.
  - [x] Falha de banco é propagada ao hosted service, sem chamada HTTP ou alteração indevida da mensagem.
  - [x] O polling de cinco segundos foi preservado.

- [x] **3.2 Preservar a tolerância a falhas de integração.** Timeout, erro de rede e non-200 continuam seguindo a política de retry do FTH-005.
  - [x] Primeira e segunda falhas retornam a mensagem para `Pending` com `NextAttemptAt` cinco segundos à frente.
  - [x] Terceira falha vai para `Failed` e não volta a ser elegível automaticamente.
  - [x] Dados ausentes e payload inválido continuam sendo falhas permanentes.

**Critério de aceite:** uma falha inesperada de persistência não mata silenciosamente o `BackgroundService`, e o shutdown normal continua limpo.

### 4. Testes exigidos do FTH-005 — concluído

- [x] **4.1 Expandir os testes de Application/Domain sem criar testes próprios de API ou Infrastructure.** Os testes verificam decisões observáveis do processor e as chamadas às portas.
  - [x] Sucesso com operação `Created`.
  - [x] Sucesso com operação `Updated`.
  - [x] Customer/Application ausentes resultando em falha permanente, sem chamada à integração.
  - [x] Timeout resultando em retry.
  - [x] Erro de rede resultando em retry.
  - [x] Resposta non-200 resultando em retry.
  - [x] Backoff fixo de cinco segundos sendo solicitado ao store.
  - [x] Terceira tentativa usando `permanent = true`.
  - [x] Payload contendo `eventId` e operação correta.

- [x] **4.2 Cobrir concorrência e lease no nível permitido pelo projeto.**
  - [x] Verificar que uma conclusão com `LockId` diferente não é aceita.
  - [x] Verificar que uma falha com `LockId` diferente não altera a mensagem.
  - [x] Verificar que mensagem com lease expirado pode ser reivindicada novamente por inspeção da regra `LockedUntil <= now` do store.
  - [x] Verificar que mensagem ainda dentro do lease não é reivindicada novamente por inspeção da regra de elegibilidade do store.

- [x] **4.3 Tornar os testes determinísticos.**
  - [x] Usar relógio controlado, sem depender de `DateTimeOffset.UtcNow` real.
  - [x] Capturar argumentos enviados às interfaces para validar timestamps, `LockId`, operação e erro.
  - [x] Não considerar “compila” como evidência de que o cenário está coberto.

- [x] **4.4 Registrar a execução pendente corretamente.** O handoff diz que os testes foram criados e compilados, mas não executados pelo agente. A execução fica sob responsabilidade do usuário.

**Critério de aceite:** todos os cenários obrigatórios do contexto FTH-005 possuem teste identificável, com asserts sobre comportamento e argumentos relevantes.

### 5. Corrigir handoff, checklist e evidências — concluído

- [x] **5.1 Corrigir `03-backend-handoff.md`.**
  - [x] Remover afirmações de que `ClaimNextAsync` usa `ExecuteUpdateAsync` se isso não estiver implementado.
  - [x] Remover afirmações de que o store abre e confirma uma transação própria se o commit continuar na Application.
  - [x] Separar claramente “implementado”, “verificado por inspeção” e “não verificado por teste”.
  - [x] Atualizar a matriz BDD para refletir os testes realmente criados.
  - [x] Não marcar BDD crítico como `covered` apenas por existir uma classe ou uma constante.

- [x] **5.2 Corrigir `03-backend-checklist.md`.**
  - [x] Manter a regra de que a Application coordena `SaveChangesAsync`.
  - [x] Registrar as pendências de concorrência, lease e testes até que existam evidências.
  - [x] Não registrar `PASS` antes de revisar o comportamento efetivo.

- [x] **5.3 Corrigir o README e o log de desenvolvimento quando houver afirmações incorretas.**
  - [x] README não diz mais que o worker usa a mesma transação da requisição HTTP.
  - [x] README informa que Customer, Application e Outbox são persistidos atomicamente na requisição e que o worker processa a Outbox depois, em outro escopo.
  - [x] O log de desenvolvimento já estava coerente com essa separação e foi preservado.
  - [x] O contrato HTTP detalhado continua explicitamente sob responsabilidade do FTH-006.

**Critério de aceite:** qualquer agente sem contexto consegue distinguir código implementado, evidência estática, teste não executado e dependência futura.

### 6. Validar o envelope antes da integração — concluído

- [x] **6.1 Corrigir `OutboxEventEnvelope.TryParse`.** Um JSON com IDs válidos, mas evento/operação inválidos, é rejeitado.
  - [x] Aceitar somente `EventType == ApplicationApproved`.
  - [x] Aceitar somente `Operation == Created` ou `Operation == Updated`.
  - [x] Rejeitar `EventId`, `CustomerId` ou `ApplicationId` vazios.
  - [x] Rejeitar `OccurredAt` ausente ou inválido; o contrato interno exige valor diferente do default.
  - [x] Tratar JSON inválido sem lançar exceção para o hosted service.

- [x] **6.2 Definir o destino de payload inválido.**
  - [x] Marcar como falha permanente (`Failed`).
  - [x] Registrar mensagem de erro útil sem incluir SSN ou dados pessoais desnecessários no log.
  - [x] Não chamar `IApprovedApplicationIntegration` para payload inválido.

- [x] **6.3 Adicionar testes para payload válido e inválido.**
  - [x] Evento válido `ApplicationApproved` + `Created`.
  - [x] Evento válido `ApplicationApproved` + `Updated`.
  - [x] Event type desconhecido.
  - [x] Operation desconhecida.
  - [x] IDs vazios.
  - [x] JSON malformado.

**Critério de aceite:** somente envelopes internos válidos chegam ao carregamento de dados e à integração externa.

### 7. Validação final da entrega e integração pendente — concluído

- [x] **7.1 Revisar a fronteira do cliente HTTP.**
  - [x] Manter `IApprovedApplicationIntegration` como porta usada pela Application.
  - [x] Manter endpoint, payload HTTP detalhado e comportamento do mock sob responsabilidade do FTH-006.
  - [x] Não inventar documentação de endpoint definitivo neste card.
  - [x] Garantir que `ExternalService:BaseUrl` continue configurável por ambiente.

- [x] **7.2 Executar somente as verificações permitidas.**
  - [x] `dotnet build FundoTakeHome.slnx --no-restore` passou.
  - [x] `git diff --check` passou.
  - [x] Referências, DI, configuração e documentação foram inspecionadas.
  - [x] xUnit, Vitest, Playwright e outras suítes automatizadas não foram executadas.

- [x] **7.3 Fazer a revisão final contra o FTH-005.**
  - [x] Confirmar polling de cinco segundos.
  - [x] Confirmar lease de trinta segundos.
  - [x] Confirmar commit do claim antes da chamada HTTP.
  - [x] Confirmar retry fixo e terceira falha permanente.
  - [x] Confirmar uso dos dados atuais de Customer/Application.
  - [x] Confirmar operações `Created` e `Updated`.
  - [x] Confirmar que mensagens `Failed` não são revendidas automaticamente.
  - [x] Confirmar que a documentação não declara evidência inexistente.

- [x] **7.4 Registrar o resultado para o validador final.**
  - [x] Listar arquivos revisados e evidências no handoff.
  - [x] Listar cada verificação executada e seu resultado.
  - [x] Declarar explicitamente que a suíte automatizada não foi executada.
  - [x] Declarar as pendências do FTH-006 sem tratá-las como falha do FTH-005.

**Critério de aceite:** a entrega pode ser revisada por outro agente apenas lendo este checklist, o diff, os testes e as evidências registradas.

## Resultado do checklist

- [x] Item 1 — boundary da unidade de trabalho confirmado.
- [x] Item 2 — claim e conclusão concorrentes revisados.
- [x] Item 3 — resiliência do hosted service corrigida.
- [x] Item 4 — testes obrigatórios adicionados e compiláveis; execução pendente do usuário.
- [x] Item 5 — handoff, checklist e documentação corrigidos.
- [x] Item 6 — validação do envelope corrigida.
- [x] Item 7 — validação final concluída.
