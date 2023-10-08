using CqrsVibe.Queries;
using Grpc.Core;
using SomeShop.Catalog.V1;
using SomeShop.Common.Domain;
using Money = SomeShop.Common.Proto.Money;

namespace SomeShop.Catalog.App.Api.V1;

public class GrpcService : Service.ServiceBase
{
    private readonly IQueryService _queryService;

    public GrpcService(IQueryService queryService)
    {
        _queryService = queryService;
    }

    public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        var result = await _queryService.QueryAsync(new GetProducts(Currency.RUB), context.CancellationToken);

        var response = new GetProductsResponse();
        
        foreach (var product in result.Products)
        {
            response.Products.Add(new GetProductsResponse.Types.Product
            {
                Id = product.Id.Value.ToString("D"),
                Name = product.Name,
                Price = new Money(product.PriceAmount, product.PriceCurrency)
            });
        }

        return response;
    }
}