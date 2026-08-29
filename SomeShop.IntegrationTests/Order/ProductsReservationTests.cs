using Grpc.Net.Client;
using SomeShop.Ordering.Cart.V1;
using SomeShop.Ordering.Order.V1;

using CartService = SomeShop.Ordering.Cart.V1.Service;
using GetRequest = SomeShop.Ordering.Order.V1.GetRequest;
using OrderService = SomeShop.Ordering.Order.V1.Service;

namespace SomeShop.IntegrationTests.Order;

/// The full asynchronous round trip: Ordering publishes OrderCreated, StockManagement reserves
/// against real stock and publishes the result, Ordering moves the order accordingly.
public class ProductsReservationTests
{
    private static readonly TimeSpan ReservationTimeout = TimeSpan.FromSeconds(20);

    [Test]
    public async Task CreateOrder_EnoughStock_OrderBecomesReserved()
    {
        var orderId = await CreateOrderWith(Data.GorgeousProductId.Value.ToString("D"), quantity: 1);

        Assert.AreEqual("Reserved", await WaitForStatusChange(orderId));
    }

    [Test]
    public async Task CreateOrder_NotEnoughStock_OrderBecomesFailed()
    {
        // Seeded stock holds 100 of each product.
        var orderId = await CreateOrderWith(Data.IncredibleProductId.Value.ToString("D"), quantity: 1_000);

        Assert.AreEqual("Failed", await WaitForStatusChange(orderId));
    }

    private static async Task<string> CreateOrderWith(string productId, uint quantity)
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var cartClient = new CartService.ServiceClient(channel);
        var orderClient = new OrderService.ServiceClient(channel);

        var cartId = (await cartClient.CreateAsync(new CreateRequest())).CartId;

        await cartClient.AddProductAsync(new AddProductRequest
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity
        });

        return (await orderClient.CreateAsync(new CreateOrderRequest { CartId = cartId })).OrderId;
    }

    private static async Task<string> WaitForStatusChange(string orderId)
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var orderClient = new OrderService.ServiceClient(channel);

        var deadline = DateTime.UtcNow + ReservationTimeout;
        var status = string.Empty;

        while (DateTime.UtcNow < deadline)
        {
            status = (await orderClient.GetAsync(new GetRequest { Id = orderId })).Order.Status;
            if (status != "Initial")
            {
                return status;
            }

            await Task.Delay(200);
        }

        return status;
    }
}
