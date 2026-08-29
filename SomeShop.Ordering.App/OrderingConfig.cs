// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace SomeShop.Ordering.App;

public class OrderingConfig
{
    public const string Section = "Ordering";

    /// Which wiring of the ICatalog port to use. Both satisfy the same domain port, so this
    /// changes the transport only — never the behaviour visible to the domain.
    public CatalogTransport CatalogTransport { get; set; } = CatalogTransport.InProcess;

    /// Address of the Catalog gRPC endpoint. Only used with CatalogTransport.Grpc; today it points
    /// back at this very host, and would point at a separate service once Catalog is extracted.
    public string CatalogGrpcUrl { get; set; } = "http://localhost:13001";
}

public enum CatalogTransport
{
    InProcess,
    Grpc
}
