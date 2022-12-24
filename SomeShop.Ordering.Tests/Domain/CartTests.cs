using Moq;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.Tests.Domain;

public class CartTests
{
    [Test]
    public void Create_ShouldConstructCart()
    {
        var cart = Cart.Create();
        Assert.NotNull(cart);
    }

    [Test]
    public void Add_ShouldAddItem()
    {
        const decimal expectedSum = 100M;
        var productId = ProductId.Create();
        var cart = Cart.Create();
        var catalogMock = new Mock<ICatalog>();
        catalogMock
            .Setup(x => x.GetProduct(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product(productId, new Money(expectedSum, Currency.RUB)));

        Assert.DoesNotThrowAsync(async () =>
        {
            await cart.Add(productId, 1, catalogMock.Object, CancellationToken.None);
        });

        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        Assert.NotNull(item, $"Cart doesn't contain cart item with productId={productId.Value}");
        Assert.That(cart.TotalSum.Amount, Is.EqualTo(expectedSum));
        Assert.That(item!.CartId, Is.EqualTo(cart.Id));
        Assert.That(item.Id.Value, Is.Not.EqualTo(Guid.Empty));
        Assert.NotNull(cart.TotalProductsPositions);
    }
    
    [Test]
    public async Task Add_AlreadyAddedProduct_ThrowsException()
    {
        var productId = ProductId.Create();
        var cart = Cart.Create();
        var catalogMock = new Mock<ICatalog>();
        catalogMock
            .Setup(x => x.GetProduct(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product(productId, new Money(100, Currency.RUB)));

        await cart.Add(productId, 1, catalogMock.Object, CancellationToken.None);
        
        Assert.ThrowsAsync<ProductAlreadyInCartException>(async () =>
        {
            await cart.Add(productId, 1, catalogMock.Object, CancellationToken.None);
        });
    }
}