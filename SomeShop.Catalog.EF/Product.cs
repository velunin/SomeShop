using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Catalog.EF;

public class Product
{
    public ProductId Id { get; set; }
    
    public string Name { get; set; }
    
    public Money Price { get; set; }
}