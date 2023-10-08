using Moq;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.Tests.Domain;

public class OrderTests
{
    [Test]
    public void Create_ShouldConstructOrder()
    {
        var cartId = CartId.Create();
        var cartItemsMock = new List<CartItemWithActualPrice>
        {
            new(ProductId.Create(), new Quantity(1), new Money(500, Currency.EUR)),
            new(ProductId.Create(), new Quantity(1), new Money(600, Currency.EUR)),
            new(ProductId.Create(), new Quantity(1), new Money(700, Currency.EUR)),
        };

        var cartServiceMock = new Mock<ICartWithActualPrices>();
        cartServiceMock
            .Setup(x => x.GetItems(It.IsAny<CartId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItemsMock);
        
        Assert.DoesNotThrowAsync(async () =>
        {
            var order = await Order.Create(cartId, cartServiceMock.Object, CancellationToken.None);
            
            Assert.IsNotNull(order);
            Assert.That(order.Items.Count, Is.EqualTo(cartItemsMock.Count));
            Assert.That(order.TotalSum.Amount, 
                Is.EqualTo(cartItemsMock.Select(x=>x.ActualPrice.Amount * x.Quantity.Value).Sum()));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Initial));
            Assert.That(order.ReservationStatus, Is.EqualTo(ReservationStatus.Awaiting));

            var orderCreatedEvent = order.Events.FirstOrDefault(x=> x.GetType() == typeof(OrderCreated));
            Assert.IsNotNull(orderCreatedEvent);
            Assert.That(((OrderCreated)orderCreatedEvent!).OrderId, Is.EqualTo(order.Id));
        });
    }
    
    [Test]
    public void Create_EmptyCart_ThrowsException()
    {
        var cartId = CartId.Create();
        var cartServiceMock = new Mock<ICartWithActualPrices>();
        cartServiceMock
            .Setup(x => x.GetItems(It.IsAny<CartId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItemWithActualPrice>());
        
        Assert.ThrowsAsync<CartIsEmptyException>(async () =>
        {
             await Order.Create(cartId, cartServiceMock.Object, CancellationToken.None);
        });
    }
    
    [Test]
    public async Task WhenReservationOnStockConfirmed_AwaitingStatus_DoesNowThrowException()
    {
        var order = await CreateOrder();
        
        Assert.DoesNotThrow(() =>
        {
            order.WhenReservationOnStockConfirmed();
        });
        Assert.That(order.ReservationStatus, Is.EqualTo(ReservationStatus.WasReserved));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Reserved));
    }
    
    [Test]
    public async Task WhenReservationOnStockConfirmed_WasReservedStatus_ThrowsException()
    {
        var order = await CreateOrder();

        order.WhenReservationOnStockConfirmed();
        
        Assert.Throws<ChangeReservationStatusException>(() =>
        {
            order.WhenReservationOnStockConfirmed();
        });
        Assert.That(order.ReservationStatus, Is.EqualTo(ReservationStatus.WasReserved));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Reserved));
    }
    
    [Test]
    public async Task WhenReservationOnStockFailed_AwaitingStatus_DoesNowThrowException()
    {
        var order = await CreateOrder();

        Assert.DoesNotThrow(() =>
        {
            order.WhenReservationOnStockFailed();
        });
        Assert.That(order.ReservationStatus, Is.EqualTo(ReservationStatus.WasFailed));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Failed));
        Assert.That(order.FailReason, Is.EqualTo(OrderFailReason.ReservationFailed));
    }
    
    [Test]
    public async Task WhenReservationOnStockFailed_WasFailedStatus_ThrowsException()
    {
        var order = await CreateOrder();

        order.WhenReservationOnStockFailed();
        
        Assert.Throws<ChangeReservationStatusException>(() =>
        {
            order.WhenReservationOnStockFailed();
        });
    }
    
    [Test]
    public async Task WhenReservationOnStockConfirmed_WasFailedStatus_ThrowsException()
    {
        var order = await CreateOrder();

        order.WhenReservationOnStockConfirmed();
        
        Assert.Throws<ChangeReservationStatusException>(() =>
        {
            order.WhenReservationOnStockConfirmed();
        });
    }
    
    [Test]
    public async Task Checkout_WasFailedStatus_ThrowsException()
    {
        var order = await CreateOrder();

        order.WhenReservationOnStockFailed();
        
        Assert.Throws<OrderShouldBeReservedException>(() =>
        {
            order.Checkout();
        });
    }
    
    [Test]
    public async Task Checkout_AwaitingReservationStatus_ThrowsException()
    {
        var order = await CreateOrder();

        order.WhenReservationOnStockFailed();
        
        Assert.Throws<OrderShouldBeReservedException>(() =>
        {
            order.Checkout();
        });
    }
    
    [Test]
    public async Task Checkout_WasReservedStatus_ChangeStatusToCheckedOut()
    {
        var order = await CreateOrder();

        order.WhenReservationOnStockConfirmed();
        
        Assert.DoesNotThrow(() =>
        {
            order.Checkout();
        });
        Assert.That(order.Status, Is.EqualTo(OrderStatus.CheckedOut));
    }

    private static async Task<Order> CreateOrder()
    {
        var cartId = CartId.Create();
        var cartItemsMock = new List<CartItemWithActualPrice>
        {
            new(ProductId.Create(), new Quantity(1), new Money(500, Currency.EUR)),
        };

        var cartServiceMock = new Mock<ICartWithActualPrices>();
        cartServiceMock
            .Setup(x => x.GetItems(It.IsAny<CartId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItemsMock);
        
        return await Order.Create(cartId, cartServiceMock.Object, CancellationToken.None);
    }
}