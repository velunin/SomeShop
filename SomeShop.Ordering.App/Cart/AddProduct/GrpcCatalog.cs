using Grpc.Core;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Exceptions;
using SomeShop.Ordering.Domain;

using CatalogGrpc = SomeShop.Catalog.V1;

namespace SomeShop.Ordering.App.Cart;

/// The out-of-process wiring of the same <see cref="ICatalog"/> port: the Catalog contract is
/// called over gRPC. Nothing above this class changes when the wiring is switched — this is what
/// makes extracting the Catalog context into its own service an adapter swap.
public class GrpcCatalog : ICatalog
{
    private readonly CatalogGrpc.Service.ServiceClient _client;

    public GrpcCatalog(CatalogGrpc.Service.ServiceClient client)
    {
        _client = client;
    }

    public async Task<Product> GetProduct(ProductId id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetProductPriceAsync(
                new CatalogGrpc.GetProductPriceRequest { ProductId = id.Value.ToString("D") },
                cancellationToken: cancellationToken);

            return new Product(new ProductId(Guid.Parse(response.ProductId)), response.Price.ToValueObject());
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // Transport-level failures are translated here, so callers see the same error shape
            // they see with the in-process wiring.
            throw new CatalogProductNotFoundException(id);
        }
    }
}

public class CatalogProductNotFoundException : NotFoundException
{
    public CatalogProductNotFoundException(ProductId id) : base($"Product with id = '{id.Value:D}' not found")
    {
    }
}
