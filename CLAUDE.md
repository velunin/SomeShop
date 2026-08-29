# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SomeShop is a reference DDD/CQRS/Monolith-first e-commerce application built with .NET 7. The project demonstrates Domain-Driven Design patterns with CQRS architecture in a modular monolith structure.

## Architecture

The solution is organized into three main bounded contexts:
- **Ordering**: Handles cart management and order processing
- **Catalog**: Manages product information and pricing
- **StockManagement**: Owns stock levels and reserves products for orders

Each bounded context follows a layered architecture:
- **Domain**: Core business logic and entities (DDD aggregates)
- **App**: Application services, CQRS handlers, and API endpoints
- **EF**: Entity Framework data access layer
- **Contracts**: gRPC service definitions and messaging contracts

### Key Architectural Patterns
- Domain aggregates inherit from `AggregateBase` in `SomeShop.Common.Domain`
- Domain events are applied through `ApplyEvent()` method
- gRPC services are used for inter-module communication
- Kafka is used for asynchronous messaging between contexts
- Entity Framework with PostgreSQL for persistence

## Development Commands

### Build and Run
```bash
# Build the solution
task build            # or: dotnet build

# Run the API
dotnet run --project SomeShop.Api

# Start environment (PostgreSQL, Kafka via Docker)
task run-env

# Start environment + API, waiting until it is healthy
task run-app

# Stop environment
task stop-env
```

### Testing
```bash
# Unit + architecture tests (NUnit); builds the solution once, then runs
# both suites against the prebuilt output
task unit-tests

# Integration tests from the host against a containerised API (fast local loop)
task integration-tests

# Fully containerised integration test run (CI path)
task run-integration-tests
```

Always run the test suites through `--no-build` (as the tasks do): a plain
`dotnet test <project>` re-restores and recompiles the whole dependency graph,
which costs far more than the tests themselves.

### Database Migrations
```bash
# Ordering context migrations
task ordering-migrate -- add MigrationName
task ordering-migrate -- update

# Catalog context migrations  
task catalog-migrate -- add MigrationName
task catalog-migrate -- update
```

## Key Components

### Domain Models
- Aggregates extend `AggregateBase` and use domain events
- Value objects like `Money`, `ProductId`, `OrderId` provide type safety
- Domain exceptions inherit from `DomainException`
- Aggregates may take domain ports (`ICatalog`, `ICartWithActualPrices`) as arguments and be
  async because of it — see [Design Decisions](#design-decisions)

### Application Layer
- CQRS handlers organized by feature folders
- gRPC services for synchronous communication
- Kafka consumers for asynchronous event processing

### Data Access
- Each context has dedicated DbContext
- Configurations in separate files (e.g., `OrderConfiguration.cs`)
- Common EF patterns in `SomeShop.Common.EF`

### Container Builds
- A single root `Dockerfile` holds the build graph for both images: a shared
  `build` stage restores and compiles the common projects once, then the `api`
  and `tests` stages are derived from it (`target:` in the compose files)
- The build context is the repository root, so the `.dockerignore` must stay
  there — keeping `bin`/`obj` out of the context is what keeps layers cached
- The `tests` image ships compiled assemblies and runs them with `dotnet vstest`;
  it never recompiles at run time
- Compose services are wired with healthchecks and `depends_on` conditions, so
  `up --wait` is the way to wait for readiness — never a sleep

### Testing Strategy
- **Unit Tests**: Domain logic testing with NUnit and Moq
- **Integration Tests**: Full API testing with Docker containers
- **Architecture Tests**: Enforcing architectural boundaries with ArchUnit.NET

## Modularity

A bounded context is packaged as exactly one module — the design boundary and the deployment unit
coincide — and each one is a set of projects following a single naming template:

```
SomeShop.<BoundedContext>.<Layer>
```

The composition root of each is a static `Module` class, and `SomeShop.ArchTests` enumerates the
same list as `BoundedContexts`. Nothing else is a module. The rest of this file writes the
template head as `<Context>` for brevity.

- `<Context>.Domain` — aggregates, value objects, domain exceptions and the ports the
  domain needs (`ICatalog`, `ICartWithActualPrices`). No DI, no EF, no infrastructure.
- `<Context>.App` — use cases (CQRS handlers), gRPC endpoints, Kafka consumers,
  repositories and port implementations. The composition point of the context.
- `<Context>.EF` — `DbContext`, entity configurations, migrations.
- `<Context>.Contracts` — the public surface of the context: `.proto` files, Kafka topic
  names, and in-process query contracts (`InternalApi`). This is the only project other
  contexts are allowed to reference.

Shared code lives in `SomeShop.Common.*` (`Domain`, `App`, `EF`, `Proto`). `SomeShop.Api`
is the single host and composes the modules. Tests are `SomeShop.<Context>.Tests`,
`SomeShop.ArchTests` and `SomeShop.IntegrationTests`; one-off utilities are
`SomeShop.Tools.<Name>`.

Every `App` project exposes exactly one static `Module` class with `Add<Context>()` for DI,
`ConfigureConsumers()` for Kafka subscriptions (consumer group `"<context>-consumer-group"`),
and `Init()` where the context needs migrations or Dapper type handlers. `Program.cs` is the
only place that calls them — adding a module means adding one line there.

### Naming Conventions
- Namespaces follow the project and the aggregate, not the full folder path:
  `Cart/AddProduct/AddProduct.cs` is `namespace SomeShop.Ordering.App.Cart`. Use-case folders
  group files, they do not appear in the namespace.
- One use case = one folder `<Aggregate>/<UseCase>/` with one file `<UseCase>.cs` holding both
  the message and its handler: `AddProduct : ICommand` + `AddProductHandler`,
  `GetCart : IQuery<GetCartModel>` + `GetCartHandler` + the `GetCartModel` read model.
- Reactions to domain events live in `<UseCase>/When<Event>/` and are named after what they do
  (`SendToOutbox`, `ClearCart`), implementing `IEventHandler<TEvent>`.
- Kafka consumers are `<Event>Consumer : IConsumer`, placed in a `When…Received/` folder; their
  payload is the `<Event>Message` proto of the producing context.
- gRPC: `Api/<Aggregate>/V1/GrpcService.cs` implements `service Service` from
  `Contracts/Protos/V1/<aggregate>.proto` (`package <aggregate>`,
  `option csharp_namespace = "SomeShop.<Context>.<Aggregate>.V1"`), with `<UseCase>Request` /
  `<UseCase>Response` messages.
- In-process cross-context reads: `Contracts/InternalApi/<Query>.cs` declaring
  `<Query> : IQuery<<Query>Model>`; the owning context implements `<Query>Handler` under
  `Api/Internal/`.
- Kafka topics are constants in `Contracts/Topics.cs`, valued `someshop.<context>.<event>-v<N>`.
- EF: `<Context>DbContext` with `HasDefaultSchema("<context>")`, one `<Entity>Configuration`
  per entity, snake_case columns. Ids are `readonly struct`s in `Common.Domain/Ids`.
- Exceptions: domain rules throw `<X>Exception : DomainException` declared next to the
  aggregate; use cases throw `<X>NotFoundException : NotFoundException` declared in the same
  file as the query.

### Allowed Dependencies

`SomeShop.ArchTests/BoundariesTests.cs` enforces exactly two rules over the loaded
`SomeShop.*` assemblies:

1. `<Context>.Domain` may depend only on itself and `SomeShop.Common.Domain`.
2. Any `SomeShop.<Context>.*` type may depend only on the same context, `SomeShop.Common.*`,
   and any `*.Contracts`.

What that means in practice:

- Within a context dependencies point one way, `App` → `EF` → `Domain`: `Domain` references
  only `Common.Domain`, `EF` its own `Domain` plus `Common.EF`, and `App` everything of its own
  context. `Contracts` stands apart and references only `Common.Proto`.
- Between contexts, only `*.Contracts` — proto/gRPC, topic names, and `InternalApi` queries
  dispatched in-process by CqrsVibe. `Ordering.App` reaches Catalog through
  `Catalog.Contracts.InternalApi`, never through `Catalog.App`.
- `SomeShop.Api` references the three `*.App` projects and nothing else. Integration tests go
  through the API over gRPC; unit tests reference only a `Domain` project.
- A new bounded context must be added to the `BoundedContexts` array in `BoundariesTests.cs`,
  otherwise its boundaries are unchecked.

Deviations that exist in the tree — do not copy them:

- `SomeShop.Ordering.EF` has a project reference to `SomeShop.Catalog.EF` that is only used by a
  dead `using Product = SomeShop.Catalog.EF.Product;` alias in `OrderingDbContext`. There is no
  type-level dependency, which is why the (IL-based) arch tests stay green.
- `SomeShop.Catalog.EF` references `SomeShop.Common.App` for `PostgresConfig` — an EF layer
  reaching into an App layer, permitted only because the target is `Common`.
- The `SomeShop.Common` directory holds a stray `Class1` project that is not in the solution and
  is referenced by nothing. Shared code goes into `SomeShop.Common.<Layer>`, not there.

## Design Decisions

The codebase takes a couple of explicit positions that differ from the most common tactical
advice. They are an approach, not a compromise or an oversight — keep to them when adding code.

Separately, the places where the reference deliberately stops short (fake outbox, no DLQ/retry/
idempotency in the consumer, no reservation timeout) carry an `Out of scope by design` comment.
Treat those as intentional: do not report them as findings and do not implement them unasked.

### Rich Domain Model

Business logic belongs to the aggregate, including the parts that need data the aggregate does
not hold. Aggregates reach that data through ports declared in the domain layer and receive the
port as an argument:

- `Cart.Add(productId, quantity, ICatalog, ct)` asks the catalog for the product itself.
- `Order.Create(cartId, ICartWithActualPrices, ct)` is an async factory that pulls the cart
  items with their current prices.

The ports (`SomeShop.Ordering.Domain/Cart/ICatalog.cs`,
`SomeShop.Ordering.Domain/Order/ICartWithActualPrices.cs`) live in `Domain`; their adapters
(`Ordering.App/Cart/Catalog.cs`, `Ordering.App/Order/CreateOrder/CartWithActualPrices.cs`) live
in `App` and are the only place that knows about Dapper, EF or another context. That is why
aggregate methods are async — it is a consequence of the choice, not an accident.

The default is to inject the port and let the aggregate fetch what it needs itself.
Pre-resolving that data in a command handler and passing plain values into the aggregate is not
forbidden and is a fine choice where it reads better — it is simply not the default shape here,
because the domain layer stays more expressive when a rule and the data it needs sit together.

### The ICatalog Port Has Two Wirings

`ICatalog` is deliberately implemented twice, and the switch between them is the demonstration
that a context can be extracted without touching anything above the port:

- `Ordering.App/Cart/AddProduct/InProcessCatalog.cs` — dispatches the Catalog contract as a CQRS
  query inside the same host.
- `Ordering.App/Cart/AddProduct/GrpcCatalog.cs` — calls the Catalog contract over gRPC and
  translates transport failures back into the same error shape.

`Module.AddCatalogPort` picks one from `Ordering:CatalogTransport` (`InProcess` | `Grpc`). The
domain, the handlers and the tests are identical either way — the integration suite passes
against both, and CI can run it twice:

```bash
task run-integration-tests                                   # InProcess
ORDERING_CATALOG_TRANSPORT=Grpc task run-integration-tests   # gRPC
```

Keep both wirings working when changing anything around `ICatalog`. Note that the scope of the
demonstration is this one port: `ICartWithActualPrices` still reads Catalog in-process.

### Facts and Commands Are Both Available — Pick Per Relationship

How two contexts interact is a tactical decision, not a rule. What is actually being decided is the
direction of the dependency and who owns the interaction contract:

- **A fact** (integration event over Kafka): the context that knows something owns the contract and
  publishes it, knowing nothing about who reacts. The dependency points from the reactor to the
  publisher's `*.Contracts`.
- **A command** (a call into another context, in-process or over gRPC, typically fed by an outbox):
  the context that performs the work owns the contract, knowing nothing about who calls it. The
  dependency points from the caller to the performer's `*.Contracts`, and the call is synchronous.

Both shapes are already in the tree. The reservation flow is choreographed with two facts —
Ordering publishes `OrderCreated`, StockManagement publishes `OrderProductsReservationResultMessage`
— so each context references the other's `*.Contracts`. The price lookup is a call into a contract
Catalog owns.

Whichever shape a new interaction takes, the contract carries what the other side needs and nothing
more: `ReservationFailReason` is persisted but never published, because why a reservation failed is
StockManagement's model. And a `Reservation` is keyed by `OrderId`, so a redelivered `OrderCreated`
cannot reserve twice.

### Cross-Aggregate Commands Within a Context

A bounded context may hold several aggregates: splitting one is a normal tactical move when it
follows how people actually think about the process, or when it keeps each set of business rules
short enough to read comfortably.

Within a single bounded context it is acceptable for a domain event handler to execute a command
on another aggregate inside the same transaction. Domain events are the mechanism for that, and
consistency between aggregates of one context is immediate rather than eventual.

Across contexts this does not apply: consistency there stays asynchronous and goes through
`*.Contracts` (Kafka topics, proto messages), never through a shared transaction.