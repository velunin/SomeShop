using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.Tests.Domain;

public class ProductTests
{
    [Test]
    public void Ctor_NegativePrice_ThrowsException()
    {
        Assert.Throws<InvalidProductPriceException>(() =>
            _ = new Product(ProductId.Create(), new Money(-1, Currency.RUB)));
    }

    [Test]
    public void Ctor_PositivePrice_ConstructsProduct()
    {
        Assert.DoesNotThrow(() => _ = new Product(ProductId.Create(), new Money(200, Currency.RUB)));
    }
}