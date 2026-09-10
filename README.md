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

Os testes foram preparados para xUnit, Shouldly e `WebApplicationFactory`. A execução da suíte fica sob responsabilidade do usuário.

## Estrutura inicial

- `src/backend/FundoTakeHome.Api`: API .NET 10 e fundação backend.
- `tests/FundoTakeHome.Tests`: testes backend.
- `src/frontend`: placeholder reservado para Next.js.
- `src/mock`: placeholder reservado para o mock externo.
