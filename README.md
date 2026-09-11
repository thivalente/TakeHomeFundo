# Fundo TakeHome

Vídeo da solução: pendente.

## Executar tudo localmente

O frontend é uma aplicação Next.js exportada como arquivos estáticos e servida por Nginx; o mock é um serviço .NET. O caminho reproduzível para executar o ambiente completo é Docker Compose:

```bash
docker compose up --build
```

Serviços:

- Frontend: http://localhost:3317
- API: http://localhost:8317
- Swagger: http://localhost:8317/swagger
- Health check: http://localhost:8317/api/health
- Mock externo: http://localhost:4317/health
- SQLite: volume Docker `fundotakehome-data`

Portas públicas padrão do projeto:

- Frontend `3317` → container `80`
- API `8317` → container `8080`
- Mock externo `4317` → container `80`

As portas foram escolhidas para evitar as portas comuns `3000`, `4000` e `8080`.
Se alguma delas estiver ocupada, sobrescreva as portas antes de subir o ambiente.
No PowerShell:

```powershell
$env:FUNDO_FRONTEND_PORT = "3327"
$env:FUNDO_API_PORT = "8327"
$env:FUNDO_MOCK_PORT = "4327"
docker compose up --build -d
```

O `NEXT_PUBLIC_API_URL` do frontend é gerado durante o build usando a porta pública
da API escolhida em `FUNDO_API_PORT`; depois de trocar a porta, execute o build
novamente.

Para encerrar:

```bash
docker compose down
```

## Acompanhar a Outbox e o mock

Em outro terminal, acompanhe somente os eventos relevantes da API:

```powershell
docker compose logs -f --tail=0 api |
  Select-String "worker started|Outbox message|External integration"
```

Para acompanhar as chamadas recebidas pelo mock:

```powershell
docker compose logs -f --tail=0 mock
```

Uma aplicação nova deve gerar `POST /customers` com `Operation=Created`. Uma aplicação
do mesmo SSN deve gerar `PUT /customers/{customerId}` com `Operation=Updated`. Os
healthchecks do Docker e os detalhes internos do framework ficam ocultos nesses logs.

Para executar somente a API fora do Docker:

```bash
dotnet run --project src/backend/FundoTakeHome.Api --urls http://localhost:8317
```

Nesse caso, o SQLite fica em `src/backend/FundoTakeHome.Api/data/fundotakehome.db` e o serviço externo precisa estar disponível em `http://localhost:4317/`.

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
