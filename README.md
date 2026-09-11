# Fundo TakeHome

Vídeo da solução: pendente.

## Executar tudo localmente

O frontend e o mock atual são páginas estáticas servidas por Nginx; por isso, o caminho reproduzível para executar o ambiente completo é Docker Compose:

```bash
docker compose up --build
```

Serviços:

- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Health check: http://localhost:8080/api/health
- Frontend: http://localhost:3000
- Mock externo: http://localhost:4000
- SQLite: volume Docker `fundotakehome-data`

Para encerrar:

```bash
docker compose down
```

Para executar somente a API fora do Docker:

```bash
dotnet run --project src/backend/FundoTakeHome.Api --urls http://localhost:8080
```

Nesse caso, o SQLite fica em `src/backend/FundoTakeHome.Api/data/fundotakehome.db` e o serviço externo precisa estar disponível em `http://localhost:4000/`.

## Testes

```bash
dotnet test
```

Os testes usam xUnit, Shouldly, Moq e NetArchTest. Eles cobrem regras de decisão, domínio, casos de uso do envio e processamento da Outbox.

## Dados para demonstração

- SSNs `000000000` a `999999999` com todos os nove dígitos iguais são blacklistados e devem ser negados.
- Use um SSN válido diferente desses, por exemplo `123456789`, para aprovação.
- Use qualquer estado diferente de `NY` para aprovação.
- Use `NY` para testar a negação por estado.
- Para testar cliente retornante, envie duas solicitações aprovadas com o mesmo SSN e dados diferentes. O segundo envio deve atualizar o mesmo Customer e Application.

## Estrutura e decisões

- `src/backend/FundoTakeHome.Api/Features/SubmitApplication/Domain`: entidades, value objects, estados e erros de negócio.
- `src/backend/FundoTakeHome.Api/Features/SubmitApplication/Application`: handler, validação e rule engine. Para adicionar uma regra, implemente `IDecisionRule<DecisionRuleInput>` e registre-a no DI; as regras existentes não precisam ser alteradas.
- `src/backend/FundoTakeHome.Api/Features/SubmitApplication/Infrastructure`: persistência do Customer, Application e Outbox.
- `src/backend/FundoTakeHome.Api/Features/ApprovedApplicationDelivery/Application`: coordenação do processamento assíncrono por interfaces, sem conhecer EF ou o endpoint externo.
- `src/backend/FundoTakeHome.Api/Features/ApprovedApplicationDelivery/Infrastructure`: leitura dos dados atuais e cliente HTTP da integração.
- `src/backend/FundoTakeHome.Api/BackgroundServices`: polling da Outbox a cada cinco segundos, criando um escopo de DI por ciclo.
- `tests/FundoTakeHome.Tests`: testes de Domain, Application e regras arquiteturais.

Na aprovação, Customer, Application e OutboxMessage são persistidos na mesma unidade de trabalho. A requisição HTTP não chama o serviço externo. Depois, o BackgroundService reivindica a mensagem com lease de 30 segundos, confirma o claim antes do HTTP e envia os dados atuais do banco. Falhas temporárias voltam para `Pending` com backoff fixo de cinco segundos; a terceira falha vira `Failed`. A entrega é at-least-once, então o consumidor externo deve tratar `eventId` de forma idempotente.

O contrato HTTP detalhado do mock permanece no FTH-006. O cliente usa `ExternalService:BaseUrl`, aceita somente HTTP 200 como sucesso e encaminha a operação `Created` ou `Updated` no payload.

## Trade-offs

- SQLite foi escolhido para manter uma transação real sem infraestrutura adicional.
- O processamento assíncrono fica na própria API porque o fluxo é pequeno e não exige um processo Worker separado.
- Não foram adicionados broker, Polly, circuit breaker ou dead-letter queue; retries são deliberadamente simples e limitados a três tentativas.
