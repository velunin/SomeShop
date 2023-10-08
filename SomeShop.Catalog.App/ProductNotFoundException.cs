using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Exceptions;

namespace SomeShop.Catalog.App;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(ProductId id) : base($"Product with id = '{id.Value:D}' not found")
    {
    }
}