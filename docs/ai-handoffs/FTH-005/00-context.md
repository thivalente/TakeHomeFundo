# Contexto do card — FTH-005

## [FTH-005] Background Event Worker

Após uma aplicação ser aprovada, a API deve persistir Customer, Application e OutboxMessage de forma atômica. A requisição HTTP não deve chamar o serviço externo diretamente; um worker em background deve processar o evento persistido.

### Objetivo

Implementar um worker simples e tolerante a falhas usando transactional outbox, polling, leases, retries e entrega idempotente ao mock externo.

### Escopo

- Implementar um `BackgroundService` hospedado na API existente.
- Consultar a Outbox SQLite a cada 5 segundos.
- Reivindicar atomicamente mensagens elegíveis, alterando `Pending` para `Processing` e preenchendo `LockId` e `LockedUntil`.
- Usar lease de 30 segundos e permitir nova reivindicação após expiração.
- Confirmar a reivindicação antes da chamada HTTP.
- Carregar Customer e Application atuais pelos IDs do evento.
- Montar o payload externo com os dados atuais do banco.
- Enviar o evento por uma interface de integração; endpoint e contrato HTTP ficam no FTH-006.
- Diferenciar operações `Created` e `Updated`.
- Marcar como `Processed` somente se o status ainda for `Processing` e o `LockId` ainda corresponder.
- Registrar `Attempts`, `LastError`, `ProcessedAt`, `LockedUntil` e `LockId`.

### Estados

`Pending -> Processing -> Processed`

`Processing -> Pending` na primeira ou segunda falha.

`Processing -> Failed` na terceira falha.

Mensagens `Failed` não devem ser tentadas novamente automaticamente.

### Falhas e retries

- Incrementar `Attempts` ao reivindicar a mensagem.
- Timeout HTTP de 5 segundos por tentativa.
- Somente HTTP 200 representa sucesso.
- Erros de rede, timeout e respostas diferentes de 200 são falhas.
- Registrar o erro e usar backoff fixo de 5 segundos em `NextAttemptAt`.
- Na terceira falha, marcar como `Failed` e manter o erro final.
- Não usar Polly, broker, circuit breaker ou infraestrutura adicional de retry.
- Customer ou Application ausente tornam a mensagem `Failed`.

### Fora do escopo

Rule Engine, aprovação/negação, persistência inicial do aprovado, mock externo e contrato HTTP detalhado do FTH-006, frontend, autenticação, broker real, exactly-once, circuit breaker, dead-letter queue, dashboards e replay manual.

### Testes exigidos

Sucesso, retry por timeout/erro de rede/non-200, backoff, falha na terceira tentativa, recuperação de lease expirado, dados ausentes, conclusão protegida por `LockId` e chamadas para operações `Created` e `Updated`.

### Dependências

FTH-001, FTH-002 e FTH-006.
