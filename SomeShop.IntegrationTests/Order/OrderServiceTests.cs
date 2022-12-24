using Grpc.Core;
using Grpc.Net.Client;
using SomeShop.Ordering.Cart.V1;
using SomeShop.Ordering.Order.V1;

using CartService = SomeShop.Ordering.Cart.V1.Service;
using GetRequest = SomeShop.Ordering.Order.V1.GetRequest;
using OrderService = SomeShop.Ordering.Order.V1.Service;


namespace SomeShop.IntegrationTests.Order;

public class OrderServiceTests
{
    [Test]
    public void GetOrder_NotExistingOrderId_ReturnsNotFoundError()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new OrderService.ServiceClient(channel);

        var ex = Assert.ThrowsAsync<RpcException>(async () => await client.GetAsync(new GetRequest
        {
            Id = Guid.NewGuid().ToString("D")
        }));
        
        Assert.AreEqual(StatusCode.NotFound, ex!.StatusCode);
    }
    
    [Test]
    public async Task CreateOrder_FromCartWithProducts_ShouldCreateOrder()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var orderClient = new OrderService.ServiceClient(channel);

        var cartId = await CreateCartWithIncredibleProduct();

        var orderId = string.Empty;
        Assert.DoesNotThrowAsync(async () =>
        {
            var res = await orderClient.CreateAsync(new CreateOrderRequest
            {
                CartId = cartId
            });
            
            orderId = res.OrderId;
        });

        Ordering.Order.V1.Order? order = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            var res = await orderClient.GetAsync(new GetRequest
            {
                Id = orderId
            });

            order = res.Order;
        });

        Assert.NotNull(order);
    }

    private static async Task<string> CreateCartWithIncredibleProduct()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var cartClient = new CartService.ServiceClient(channel);
        
        var cartId = (await cartClient.CreateAsync(new CreateRequest())).CartId;

        await cartClient.AddProductAsync(new AddProductRequest
        {
            CartId = cartId,
            ProductId = Data.IncredibleProductId.Value.ToString("D"),
            Quantity = 1
        });

        return cartId;
    }
}