# Fundo Take-Home

Demo video: [Watch the public demonstration](https://www.youtube.com/watch?v=ABLkDLNTMMY)

Repository: [https://github.com/thivalente/TakeHomeFundo](https://github.com/thivalente/TakeHomeFundo)

Project board: [GitHub Project](https://github.com/users/thivalente/projects/4/views/1)

## Development approach and key decisions

This repository implements the loan application flow requested in the take-home challenge: a user submits a form, the backend evaluates eligibility, approved applications are persisted, and a background worker delivers the approved data to a local external-service mock.

The solution uses .NET/C#, Next.js, SQLite, a local mock service, and Docker Compose.

I used AI throughout the planning, implementation, and review process, including to build the Next.js frontend. I remained responsible for the scope, product decisions, technical choices, and validation against `REQUIREMENTS.md`, the acceptance criteria, and the running application.

The development workflow was:

1. Read `REQUIREMENTS.md` and define the required flows and acceptance points.
2. Plan the architecture and implementation details.
3. Organize the work into tickets and create the GitHub board.
4. Implement the tickets sequentially.
5. Continuously review the result against the requirements and the existing code.

The main decisions were:

- **Strategy-based Rule Engine:** each eligibility rule is isolated, and a new rule can be added without modifying existing rules. To add one, implement `IDecisionRule<DecisionRuleInput>` and register it in dependency injection.
- **Transactional Outbox:** `Customer`, `Application`, and `OutboxMessage` are persisted in the same unit of work. The external HTTP call happens asynchronously after the request completes.
- **EF Core transaction:** the approval flow uses one `SaveChangesAsync` call. EF Core wraps the inserts or updates in an implicit transaction, so a failure while saving the customer, application, or outbox message rolls back the complete operation. The same applies to returning customers.
- **Clean Architecture in one project:** the API, Application, Domain, and Infrastructure boundaries are kept inside one executable. Dependencies still point inward, without creating separate projects only for formal separation.
- **Vertical Slice organization:** code is grouped by use case, keeping related behavior close and avoiding generic layers that do not add value.
- **NetArchTest:** architecture tests validate that the intended boundaries remain respected within the single backend project.
- **Minimal API:** the application has few endpoints, so the HTTP boundary stays small and thin.
- **Worker inside the API:** a hosted background service is enough for this scope and avoids an unnecessary additional service.
- **SQLite and Docker Compose:** SQLite provides real transactions with minimal setup, while Compose makes the API, frontend, mock, and database reproducible locally.
- **SSN as returning-customer identity:** an approved submission with an existing SSN updates the existing customer and application instead of creating duplicates. Its outbox event causes an update in the external service.

## Functional flow

1. The user fills in the Next.js form.
2. The API validates the request.
3. The Rule Engine evaluates all eligibility rules.
4. If approved, the application creates or updates the `Customer` and `Application`, then records an outbox event in the same transaction.
5. The background worker processes the event outside the original HTTP request.
6. The mock external service receives `POST /customers` for a new customer or `PUT /customers/{customerId}` for a returning customer.

Denied applications are redirected to the frontend `/denied/` page. The page shows the denial reasons and the application does not create or update database records or outbox messages. If both denial rules match, both reasons are shown.

The current denial rules are:

- State is `NY`.
- SSN is blacklisted. The seeded blacklist contains every nine-digit value with the same repeated digit, from `000000000` through `999999999`.

## Run the complete application

Run the complete environment with Docker Compose from the repository root:

```bash
docker compose up --build
```

The services are available at:

| Service | URL |
|---|---|
| Frontend | http://localhost:3317 |
| API | http://localhost:8317 |
| Swagger | http://localhost:8317/swagger |
| API health | http://localhost:8317/api/health |
| External mock health | http://localhost:4317/health |

The API stores SQLite data in the repository bind mount `.docker-data`, mounted as `/data` inside the API container. The database file used by the running Docker API is `/data/fundotakehome.db`, which corresponds to `./.docker-data/fundotakehome.db` on the host. This is not the same file as the database used by a locally launched API (`src/backend/FundoTakeHome.Api/data/fundotakehome.db`). Close database viewer tools before starting the API because SQLite file locking on a Windows bind mount can prevent migrations from acquiring the database lock.

To verify the database used by the running Docker API, inspect the host-side file directly:

```powershell
@'
import sqlite3
db = r'.docker-data\\fundotakehome.db'
with sqlite3.connect(db) as connection:
    for table in ('Customers', 'Applications', 'OutboxMessages'):
        print(table, connection.execute(f'SELECT COUNT(*) FROM {table}').fetchone()[0])
'@ | python -
```

### Configure the public ports

The default ports can be changed with these environment variables:

- `FUNDO_FRONTEND_PORT`: frontend port, default `3317`.
- `FUNDO_API_PORT`: API port, default `8317`.
- `FUNDO_MOCK_PORT`: external mock port, default `4317`.

PowerShell example:

```powershell
$env:FUNDO_FRONTEND_PORT = "3327"
$env:FUNDO_API_PORT = "8327"
$env:FUNDO_MOCK_PORT = "4327"
docker compose up --build -d
```

To stop the services:

```bash
docker compose down
```

## Observe the asynchronous integration

After submitting an approved application, use a second terminal to observe the API and Outbox worker, and a third terminal to observe the mock service.

API and Outbox logs:

```powershell
docker compose logs -f --tail=0 api | Select-String "worker started|Outbox message|External integration"
```

Mock-service requests:

```powershell
docker compose logs -f --tail=0 mock
```

An approved new customer should produce `POST /customers`; a returning customer should produce `PUT /customers/{customerId}`. The mock masks SSNs in its logs.

## Tests

Run the .NET test suite from the repository root:

```bash
dotnet test
```

Run only the endpoint integration tests with:

```bash
dotnet test tests/FundoTakeHome.Tests/FundoTakeHome.Tests.csproj --filter FullyQualifiedName~SubmitApplicationEndpointIntegrationTests
```

The suite covers:

- Rule Engine decisions, request validation, and domain behavior;
- the application handler, including the returning-customer path;
- Outbox event parsing and processing;
- endpoint-level HTTP integration tests through an ASP.NET Core test host;
- architecture constraints using `NetArchTest`.

The endpoint integration tests use a temporary SQLite database and apply the existing migrations. They intentionally do not use EF Core InMemory because the tests must exercise relational constraints and real SQLite persistence. The `OutboxBackgroundService` is disabled by the test host so persistence assertions remain deterministic; Outbox delivery behavior remains covered by the existing Outbox unit tests.

The endpoint tests cover:

- approved new customer: HTTP `201`, response identifiers, `Location` header, and Customer/Application/Pending Outbox persistence;
- New York denial, blacklisted SSN denial, and combined denial: HTTP `422`, denial reasons, and no persistence;
- invalid request: HTTP `400` with field-level error codes and messages, and no persistence;
- returning customer: first request HTTP `201`, second request HTTP `200`, stable identifiers, updated values, no duplicates, and an `Updated` Outbox event;
- transactional rollback for new and returning customers: HTTP `500`, standardized unexpected-error response, and no partial persistence when Outbox saving fails.

There is no separate frontend test script in `src/frontend/package.json`; the frontend is validated manually through the required user flows.

## Manual validation

Open http://localhost:3317 after starting the environment. For all scenarios, use a positive requested amount below `$1,000,000`.

| Scenario | State | SSN | Expected result |
|---|---|---|---|
| Approved application | `CA` | `123456789` | HTTP `201`; creates Customer, Application, and Outbox message |
| New York denial | `NY` | `123456789` | HTTP `422`; no persistence |
| Blacklisted SSN | `CA` | `000000000` | HTTP `422`; no persistence |
| Combined denial | `NY` | `000000000` | HTTP `422`; both denial reasons |
| Returning customer | `CA` | same approved SSN | HTTP `200`; updates existing records |

For the returning-customer scenario, submit the approved case once, then submit it again with the same SSN and different customer data and/or requested amount. The second delivery should use `PUT /customers/{customerId}`.

The form sends First name, Last name, Address, State, Company name, Requested amount, and SSN to `POST /api/applications`.

## Input rules defined by this implementation

Some input rules were defined because the challenge did not specify them:

- SSN accepts nine digits, with or without hyphens. No real SSN verification, checksum validation, or external lookup is performed.
- SSNs are stored without encryption because this is a local take-home implementation using synthetic data.
- First name and last name accept up to 100 characters each; company name up to 200; address up to 300.
- Requested amount must be greater than `$0` and less than `$1,000,000`.
- The state list contains the 50 US states and `DC`.

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for the project structure, rule engine, transaction and Outbox behavior, delivery semantics, and trade-offs. See [REQUIREMENTS.md](REQUIREMENTS.md) for the original challenge statement.

## Known limitations

- The worker polls the Outbox every five seconds and retries a failed delivery up to three times.
- The Outbox worker currently loads pending candidates into memory because of the local SQLite date filtering and ordering limitation. This is acceptable for the local evaluation, but a higher-volume deployment should use indexed, database-filterable timestamps.
- The frontend and backend maintain separate copies of the US state-code list. The backend remains authoritative, but changing the list requires updating both locations.
- Authentication, metrics, tracing, alerting, and structured logging are outside the take-home scope.
- The external mock is local and exists only to demonstrate the integration contract. A production system could use a dedicated broker or worker service, but that would add operational complexity without improving this small local evaluation.
