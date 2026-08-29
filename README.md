# SomeShop

A reference DDD/CQRS/Monolith-first project.

Three bounded contexts — Ordering, Catalog and StockManagement — live in a single host
(`SomeShop.Api`) as separate module projects. They talk to each other only through
`*.Contracts`: proto/gRPC and Kafka topics for asynchronous flows, in-process CQRS queries
(`InternalApi`) for synchronous reads. The boundaries are enforced by architecture tests
(`SomeShop.ArchTests`), not by convention.

## Layout

A bounded context here is packaged as exactly one module — the design boundary and the deployment
unit coincide, which is what makes extracting one of them mechanical. One naming template covers
every project of the system:

```
SomeShop.<BoundedContext>.<Layer>
```

which gives:

```
SomeShop.Api                          the only host — composes the modules

SomeShop.Ordering.Contracts           proto + Kafka topic names  ← other contexts may reference this
SomeShop.Ordering.Domain              aggregates, value objects, ports
SomeShop.Ordering.EF                  DbContext, configurations, migrations
SomeShop.Ordering.App                 use cases, gRPC endpoints, Kafka consumers, adapters

SomeShop.Catalog.Contracts            proto + InternalApi (in-process contracts)
SomeShop.Catalog.EF
SomeShop.Catalog.App

SomeShop.StockManagement.Contracts    proto + Kafka topic names
SomeShop.StockManagement.Domain       StockItem, Reservation
SomeShop.StockManagement.EF
SomeShop.StockManagement.App

SomeShop.Common.{Domain,App,EF,Proto} shared building blocks
SomeShop.ArchTests                    the two rules that keep the above honest
```

The composition root of each module is a static `Module` class, and the architecture tests
enumerate the same list as `BoundedContexts`.

Inside a context the application layer is organised by use case, one folder per use case, with the
message and its handler in one file:

```
SomeShop.Ordering.App/
├── Api/                                        # this context's gRPC endpoints
│   ├── Cart/V1/GrpcService.cs
│   └── Order/V1/GrpcService.cs
├── Cart/
│   ├── AddProduct/
│   │   ├── AddProduct.cs                       # command + handler
│   │   ├── InProcessCatalog.cs                 # ICatalog port, in-process wiring
│   │   └── GrpcCatalog.cs                      # ICatalog port, gRPC wiring
│   ├── GetCart/GetCart.cs                      # query + handler + read model
│   └── CartRepository.cs
├── Order/
│   ├── CreateOrder/
│   │   ├── CreateOrder.cs
│   │   └── WhenOrderCreated/SendToOutbox.cs    # reaction to a domain event
│   ├── Checkout/WhenCheckedOut/ClearCart.cs
│   └── WhenReceivedEvent/ProductsReservationResultConsumer.cs
└── Module.cs                                   # DI, Kafka subscriptions, migrations
```

## Design decisions

These positions are deliberate and differ from the most common tactical advice.

**Rich domain model.** Aggregates own their business logic, including the parts that need data
they do not hold: `Cart.Add(...)` takes an `ICatalog`, `Order.Create(...)` takes an
`ICartWithActualPrices`, and both are async because of it. The ports are declared in the domain
layer, their adapters live in the application layer. Pre-resolving that data in a handler and
passing plain values into the aggregate is not forbidden either — injecting the port is simply
the default, because the domain layer stays more expressive that way.

**Cart and Order are two aggregates of one subdomain.** They are split for tactical reasons:
people perceive a cart as something preliminary, separate from the final checkout process, and
two smaller aggregates keep each description of the business rules easier to read. Both belong
to the Ordering context, so consistency between them is kept transactionally — the
`OrderCheckedOut` handler that clears the cart runs in the same transaction on purpose.
Consistency *across* contexts is a different matter and stays asynchronous.

**Facts and commands are both fine — it is a tactical choice per relationship.** Today the
reservation flow is choreographed with integration events: Ordering publishes `OrderCreated`,
StockManagement reserves against real stock and publishes the outcome back. Driving the same flow
as a command — outbox, then a call into the other context to execute it — is an equally legitimate
shape, picked where it expresses the relationship better.

What is actually being chosen is the direction of the dependency and who owns the interaction
contract:

- **A fact.** The context that knows something owns the contract and publishes it, knowing nothing
  about who reacts. The dependency points from the reactor to the publisher's `*.Contracts`.
- **A command.** The context that performs the work owns the contract, knowing nothing about who
  calls it. The dependency points from the caller to the performer's `*.Contracts`, and the call
  itself is synchronous — in-process or over gRPC.

Both are in the tree: the reservation round trip is two facts, so Ordering and StockManagement each
reference the other's contracts, while asking Catalog for a price is a call into a contract Catalog
owns. Either way the contract carries what the other side needs and nothing more —
`ReservationFailReason` is stored but never published, because why a reservation failed is
StockManagement's model, not anyone else's.

**The Catalog dependency is wired two ways.** `ICatalog` has an in-process implementation and a
gRPC one; `Ordering:CatalogTransport` selects between them and nothing above the port changes.
The integration suite passes against both wirings — `ORDERING_CATALOG_TRANSPORT=Grpc
task run-integration-tests` runs it against the out-of-process one — so "extracting a context is
an adapter swap" is something you can run, not just read.

**The CQRS plumbing is replaceable; the boundary rules are the point.** Use cases are written as
CqrsVibe handlers, and the in-process contracts in `*.Contracts/InternalApi` are declared as
`IQuery<T>`, so the library reaches the contract assemblies too. Nothing structural depends on it.
The shape is simply *the message is the contract, the handler is the implementation in the
application layer*, and any mediator-style dispatcher — MediatR, Brighter, thirty lines of your
own — gives you the same thing; swapping one for another is mechanical.

If you would rather have no dispatcher at all, the more traditional shape has identical boundary
properties: declare an application service interface in the contracts assembly — say
`ICatalogQueries` with a `GetProductPrice(...)` method instead of the `GetProductPriceById` query
type — and implement it in the application layer. The consumer then depends on an interface from
`*.Contracts` exactly as it depends on a query type today, the architecture tests are unaffected,
and the extraction story is unchanged: the interface gets a gRPC-backed implementation instead of
an in-process one. What matters here is where the contract lives and who is allowed to reference
it, not how the call is dispatched.

In code, the in-process contract and its implementation look like this today:

```csharp
// SomeShop.Catalog.Contracts/InternalApi — the contract, referenced by other contexts
public class GetProductPriceById : IQuery<GetProductPriceModel>
{
    public GetProductPriceById(ProductId id) => Id = id;
    public ProductId Id { get; }
}

// SomeShop.Catalog.App/Api/Internal — the implementation, private to Catalog
public class GetProductPriceByIdHandler : IQueryHandler<GetProductPriceById, GetProductPriceModel>
{
    public async Task<GetProductPriceModel> HandleAsync(
        IQueryHandlingContext<GetProductPriceById> context, CancellationToken cancellationToken) { ... }
}

// SomeShop.Ordering.App — the consumer, behind the ICatalog domain port
var product = await _queryService.QueryAsync(new GetProductPriceById(id), cancellationToken);
```

The same boundary without a dispatcher, if you prefer the older shape:

```csharp
// SomeShop.Catalog.Contracts/InternalApi — the contract is an interface instead of a message
public interface ICatalogQueries
{
    Task<GetProductPriceModel> GetProductPrice(ProductId id, CancellationToken cancellationToken = default);
}

// SomeShop.Catalog.App/Api/Internal — the implementation, private to Catalog
public class CatalogQueries : ICatalogQueries
{
    public async Task<GetProductPriceModel> GetProductPrice(
        ProductId id, CancellationToken cancellationToken = default) { ... }
}

// SomeShop.Ordering.App — the consumer, behind the same ICatalog domain port
var product = await _catalogQueries.GetProductPrice(id, cancellationToken);
```

Where each piece lives does not change between the two — only what is inside the files does:

```
message variant (today)                    interface variant
────────────────────────────────────────   ────────────────────────────────────────
SomeShop.Catalog.Contracts/InternalApi/    SomeShop.Catalog.Contracts/InternalApi/
└── GetProductPriceById.cs                 └── ICatalogQueries.cs
        the contract — the only thing other contexts are allowed to reference

SomeShop.Catalog.App/Api/Internal/         SomeShop.Catalog.App/Api/Internal/
└── GetProductPriceByIdHandler.cs          └── CatalogQueries.cs
        the implementation — private to Catalog, nobody may reference it

SomeShop.Ordering.App/Cart/AddProduct/     SomeShop.Ordering.App/Cart/AddProduct/
└── InProcessCatalog.cs                    └── InProcessCatalog.cs
        the consumer — an ICatalog adapter, and the domain above it sees neither variant
```

Both versions have Ordering depending on `Catalog.Contracts` and nothing else, both pass the same
architecture tests, and both extract the same way — the interface or the message gets a
gRPC-backed implementation instead of an in-process one.

Where this reference deliberately stops short — the fake outbox, the bare consumer error
handling, the missing reservation timeout — the code says so on the spot. Grep for
`Out of scope by design` to see every such place at once.

See [CLAUDE.md](CLAUDE.md) for the architecture, naming conventions, dependency rules and
development commands.
