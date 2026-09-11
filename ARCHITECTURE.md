# Architecture

This document describes the internal design of the Fundo Take-Home solution. Setup instructions and manual validation scenarios are in the [README](README.md).

## Solution shape

The backend is a single .NET Minimal API project. It keeps Clean Architecture boundaries without splitting a small take-home solution into multiple projects:

- `Endpoints`: thin HTTP boundary. The endpoint validates the request, invokes the application handler, and maps the result to HTTP `201`, `200`, `422`, or `400`.
- `Features/SubmitApplication/Application`: the submission use case, request validation, and eligibility decision rules.
- `Features/SubmitApplication/Domain`: `Customer` and `LoanApplication` entities, value objects, domain validation, business errors, and enums.
- `Features/SubmitApplication/Infrastructure`: database-backed stores used by the submission feature, including the blacklist reader.
- `Features/ApprovedApplicationDelivery/Application`: Outbox processing contracts, event parsing, delivery models, and orchestration.
- `Features/ApprovedApplicationDelivery/Infrastructure`: the SQLite-backed Outbox store and the HTTP client for the external mock.
- `Infrastructure`: shared EF Core database context, Outbox persistence models, migrations, and cross-cutting services.
- `BackgroundServices`: the hosted worker that periodically invokes the Outbox processor.

The frontend is in `src/frontend` and the local external-service mock is in `src/mock`. Docker Compose runs all three services and persists SQLite data in a named volume. A denied response is kept in frontend state and routed to `/denied/`, so denial reasons are shown on a dedicated page without putting the SSN in the URL.

The organization is Vertical Slice-oriented: code related to a use case stays close together, while shared infrastructure remains explicit. Dependencies point inward through application interfaces, so the application flow does not depend directly on EF Core or the external HTTP implementation.

## Rule Engine

Eligibility is evaluated by `IDecisionRule<DecisionRuleInput>` implementations coordinated by `DecisionRuleEngine`. The current strategies are:

1. `StateIsNyRule`: denies applications from New York.
2. `BlacklistedSsnRule`: denies SSNs found by the blacklist reader.

The engine evaluates all registered rules and returns every matching denial reason. This is why a request can receive both denial messages. To add a rule, implement the decision-rule interface and register the implementation in dependency injection; existing rules and the endpoint do not need to change.

## Approval and transaction boundary

The application handler first validates eligibility. A denied request exits before persistence, so it creates no customer, application, or Outbox message.

For an approved request, the handler:

1. Finds the customer by normalized SSN.
2. Creates a new `Customer` and `LoanApplication`, or updates the existing customer and application for a returning customer.
3. Adds an `OutboxMessage` describing the approved operation.
4. Calls `SaveChangesAsync` once through `IUnitOfWork`.

SQLite was chosen because it provides real transactions while keeping local execution simple. EF Core wraps the pending inserts or updates in an implicit transaction for that `SaveChangesAsync` call. If saving the customer, application, or Outbox message fails, the complete database operation is rolled back. The same atomicity applies to the returning-customer update path.

The external service is deliberately not called inside the request transaction. The database commit is the source of truth; the Outbox guarantees that a committed event remains available for background delivery.

The same approved SSN represents one customer and one application. A later approved submission updates both records and creates an `Updated` Outbox event instead of creating duplicates.

## Transactional Outbox and background delivery

The Outbox stores the event envelope, operation (`Created` or `Updated`), identifiers, timestamps, attempts, status, and failure information. The event contains references to the customer and application rather than relying on stale form data. Before sending, the processor loads the current records from the database.

The worker runs inside the API as a hosted background service. It polls every five seconds and processes available messages in a loop. Each message is claimed with:

- a unique lock identifier;
- a 30-second lease;
- optimistic concurrency checks when claiming and completing the message.

The lease allows another worker to reclaim a message if the original worker crashes or becomes unresponsive. The lock identifier ensures that a worker that no longer owns the lease cannot complete or fail the message. The current deployment has one worker, but these rules protect the persistence flow if processing is scaled later.

After the claim is persisted, the processor parses the event, loads the current data, and calls the integration with a five-second timeout:

- `Created` → `POST /customers`;
- `Updated` → `PUT /customers/{customerId}`.

Only HTTP `200 OK` is treated as success by the client. A successful delivery marks the message as processed. Invalid payloads or missing referenced data are permanently failed because retrying cannot repair them.

Transient failures and timeouts are recorded, returned to `Pending`, and retried with a fixed five-second polling interval. The third failed attempt becomes `Failed`. The delivery is at-least-once: if the external call succeeds but the worker fails before marking the Outbox message as processed, the message can be delivered again after the lease expires. The external consumer should therefore treat the event identifier as an idempotency key.

This implementation intentionally does not introduce a broker, Polly, circuit breaker, dead-letter queue, or a separate worker service. Those would be reasonable production evolutions, but they add infrastructure beyond the needs of this local challenge.

## External-service mock

The mock is a small local service that accepts the customer/application payload and returns HTTP `200`. It exposes `/health` for Compose health checks and the customer endpoints used by the API. Its logs mask the SSN so the integration can be observed without printing the complete sensitive field.

The API uses `ExternalService:BaseUrl`, configured by Compose as `http://mock:80/`. The frontend calls the API through the public port configured by `FUNDO_API_PORT`.

## Tests and architectural constraints

The test project covers the Rule Engine, domain behavior, request validation, the application handler, returning-customer updates, Outbox event parsing and processing, and architecture policies. `NetArchTest` verifies the intended dependency boundaries within the single backend assembly.

The current repository does not include an automated HTTP/integration test for the endpoint. The frontend is validated manually through the scenarios documented in the README; no separate frontend test script is defined.

## Trade-offs and scope

- SQLite keeps the solution reproducible and transactional without external infrastructure.
- A worker hosted in the API is sufficient for the small scope; a separate process would add deployment complexity.
- Polling, a short lease, a fixed retry interval, and three attempts are intentionally simple operational policies for the take-home.
- Authentication, metrics, tracing, alerting, and structured logging are not implemented because they are outside the requested scope.
- SSNs are synthetic test data and are stored without encryption in this local implementation; real SSN verification and external lookup are not performed.
