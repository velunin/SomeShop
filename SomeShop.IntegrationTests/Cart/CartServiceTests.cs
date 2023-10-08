using Grpc.Core;
using Grpc.Net.Client;
using SomeShop.Ordering.Cart.V1;
using SomeShop.Ordering.Order.V1;

using CartService = SomeShop.Ordering.Cart.V1.Service;
using OrderService = SomeShop.Ordering.Order.V1.Service;
using GetRequest = SomeShop.Ordering.Cart.V1.GetRequest;

namespace SomeShop.IntegrationTests.Cart;

public class CartServiceTests
{
    [Test]
    public void CreateCart_ReturnsCartId()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            var response = await client.CreateAsync(new CreateRequest());
            
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.CartId);
        });
    }
    
    [Test]
    public async Task GetCart_ExistingCartId_ReturnsCart()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);
        
        var cartId = (await client.CreateAsync(new CreateRequest())).CartId;

        Assert.DoesNotThrowAsync(async () =>
        {
            var response = await client.GetAsync(new GetRequest
            {
                CartId = cartId
            });
            
            Assert.NotNull(response.Cart);
            Assert.AreEqual(cartId, response.Cart.Id);
        });
    }
    
    [Test]
    public void GetCart_NotExistingCartId_ReturnsNotFoundError()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);

        var ex = Assert.ThrowsAsync<RpcException>(async () => await client.GetAsync(new GetRequest
        {
            CartId = Guid.NewGuid().ToString("D")
        }));
        
        Assert.AreEqual(StatusCode.NotFound, ex!.StatusCode);
    }

    [Test]
    public async Task AddProduct_ExistsProduct_ReturnsSuccessAndMakeExpectedChanges()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);
        
        const uint expectedQuantity = 1;
        var productId = Data.AmazingProductId.Value.ToString("D");
        
        var cartId = (await client.CreateAsync(new CreateRequest())).CartId;

        Assert.DoesNotThrowAsync(async () =>
        {
            await client.AddProductAsync(new AddProductRequest
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = expectedQuantity
            });
        });

        var cart = await client.GetAsync(new GetRequest { CartId = cartId });
        var addedItem = cart.Cart.Items.FirstOrDefault(x => x.ProductId == productId);
        
        Assert.NotNull(addedItem);
        Assert.AreEqual(expectedQuantity, addedItem!.Quantity);
        Assert.AreEqual(expectedQuantity, cart.Cart.TotalProductsPositions);

        var orderServiceClient = new OrderService.ServiceClient(channel);
        await orderServiceClient.CreateAsync(new CreateOrderRequest
        {
            CartId = cartId
        });
    }
    
    [Test]
    public async Task AddProduct_NotExistsProduct_ReturnsNotFoundError()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);
        
        var cartId = (await client.CreateAsync(new CreateRequest())).CartId;

        var ex = Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.AddProductAsync(new AddProductRequest
            {
                CartId = cartId,
                ProductId = Guid.NewGuid().ToString("D"),
                Quantity = 1
            });
        });

        Assert.AreEqual(StatusCode.NotFound, ex!.StatusCode);
    }

    [Test]
    public async Task ChangeQuantity_ReturnsSuccessAndMakeExpectedChanges()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);

        const uint expectedQuantity = 3;
        var productId = Data.GorgeousProductId.Value.ToString("D");
        
        var cartId = (await client.CreateAsync(new CreateRequest())).CartId;

        await client.AddProductAsync(new AddProductRequest
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = 1
        });

        Assert.DoesNotThrowAsync(async () =>
        {
            await client.ChangeProductQuantityAsync(new ChangeProductQuantityRequest
            {
                CartId = cartId,
                ProductId = productId,
                NewQuantity = expectedQuantity
            });
        });

        var cart = await client.GetAsync(new GetRequest { CartId = cartId });
        var changedItem = cart.Cart.Items.FirstOrDefault(x => x.ProductId == productId);
        
        Assert.NotNull(changedItem);
        
        var totalSum = cart.Cart.TotalSum.ToValueObject();
        var changedItemPrice = changedItem!.Price.ToValueObject();
        
        Assert.AreEqual(expectedQuantity, changedItem.Quantity);
        Assert.AreEqual(expectedQuantity, cart.Cart.TotalProductsPositions);
        Assert.AreEqual(changedItemPrice * expectedQuantity, totalSum);
    }

    [Test]
    public async Task RemoveProduct_ReturnsSuccessAndMakeExpectedChanges()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);

        var productId = Data.AmazingProductId.Value.ToString("D");
        var cartId = (await client.CreateAsync(new CreateRequest())).CartId;

        await client.AddProductAsync(new AddProductRequest
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = 1
        });
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await client.RemoveProductAsync(new RemoveProductRequest
            {
                CartId = cartId,
                ProductId = productId
            });
        });
        
        var cart = await client.GetAsync(new GetRequest { CartId = cartId });
        Assert.Zero(cart.Cart.TotalProductsPositions);
        Assert.IsEmpty(cart.Cart.Items);
    }
    
    [Test]
    public async Task Clear_ReturnsSuccessAndMakeExpectedChanges()
    {
        var channel = GrpcChannel.ForAddress(Data.ApiUrl);
        var client = new CartService.ServiceClient(channel);

        var productId = Data.AmazingProductId.Value.ToString("D");
        var cartId = (await client.CreateAsync(new CreateRequest())).CartId;

        await client.AddProductAsync(new AddProductRequest
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = 1
        });
        
        Assert.DoesNotThrowAsync(async () =>
        {
            await client.ClearAsync(new ClearRequest
            {
                CartId = cartId
            });
        });
        
        var cart = await client.GetAsync(new GetRequest { CartId = cartId });
        Assert.Zero(cart.Cart.TotalProductsPositions);
        Assert.IsEmpty(cart.Cart.Items);
    }
}