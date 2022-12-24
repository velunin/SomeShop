using Grpc.Net.Client;

using CatalogV1 = SomeShop.Catalog.V1;

namespace SomeShop.IntegrationTests.Catalog;

public class CatalogServiceTests
{
    [Test]
    public void GetProducts_ReturnsProductsList()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CatalogV1.Service.ServiceClient(channel);

        Assert.DoesNotThrowAsync(async () =>
        {
            var result = await client.GetProductsAsync(new CatalogV1.GetProductsRequest());
            Assert.NotNull(result);
        });
    }
}