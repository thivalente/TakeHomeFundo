# Fundo TakeHome

## Subir o ambiente básico

Com Docker Compose:

```bash
docker compose up --build
```

Depois, acesse:

- API: http://localhost:8080/api/health
- Swagger: http://localhost:8080/swagger
- Frontend placeholder: http://localhost:3000
- Mock service placeholder: http://localhost:4000

O banco SQLite do backend fica no volume `fundotakehome-data`.

## Executar localmente sem Docker

```bash
dotnet run --project src/backend/FundoTakeHome.Api --urls http://localhost:8080
```

## Testes

```bash
dotnet test
```

Os testes usam xUnit, Shouldly e Moq.

A cobertura está concentrada nas regras de negócio e nos casos de uso da aplicação. A execução da suíte fica sob responsabilidade do usuário.

## Estrutura da solução

- `src/backend/FundoTakeHome.Api`: host HTTP da API e endpoints.
- `src/backend/FundoTakeHome.Worker`: host do processamento assíncrono da Outbox.
- `src/backend/FundoTakeHome.Backend`: código compartilhado de Domain, Application e Infrastructure.
- `tests/FundoTakeHome.Tests`: testes de Domain e Application.
- `src/frontend`: aplicação frontend.
- `src/mock`: serviço externo simulado.

## Executar os hosts localmente

API:

```bash
dotnet run --project src/backend/FundoTakeHome.Api --urls http://localhost:8080
```

Worker:

```bash
dotnet run --project src/backend/FundoTakeHome.Worker
```

A API e o Worker usam o mesmo banco SQLite e compartilham o código de Domain, Application e Infrastructure pelo projeto `FundoTakeHome.Backend`.
