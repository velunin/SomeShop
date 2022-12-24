using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
// ReSharper disable UnusedAutoPropertyAccessor.Local

// ReSharper disable PropertyCanBeMadeInitOnly.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace SomeShop.Ordering.Domain;

public class Cart : AggregateBase
{
    public static Cart Create()
    {
        return new Cart { Id = CartId.Create() };
    }

    public async Task Add(ProductId productId, Quantity quantity,
        ICatalog catalog, CancellationToken cancellationToken)
    {
        EnsureThatProductNotYetAdded(productId);

        var product = await catalog.GetProduct(productId, cancellationToken);

        _items.Add(new CartItem(Id, quantity, product));
        RecalculateTotals();
    }

    public void ChangeQuantity(ProductId productId, Quantity newQuantity)
    {
        var item = _items.SingleOrDefault(x => x.ProductId == productId);
        if (item == default)
        {
            return;
        }

        item.Quantity = newQuantity;
        RecalculateTotals();
    }

    public void Remove(ProductId productId)
    {
        var itemToRemove = _items.FirstOrDefault(x => x.ProductId == productId);
        if (itemToRemove == null)
        {
            return;
        }

        _items.Remove(itemToRemove);
        RecalculateTotals();
    }

    public void Clear()
    {
        _items = new List<CartItem>();
        RecalculateTotals();
    }

    private void EnsureThatProductNotYetAdded(ProductId productId)
    {
        if (_items.Any(x => x.ProductId == productId))
        {
            throw new ProductAlreadyInCartException();
        }
    }

    private void RecalculateTotals()
    {
        TotalProductsPositions = 0;
        var totalSum = 0M;

        foreach (var cartItem in _items)
        {
            TotalProductsPositions += cartItem.Quantity.Value;
            totalSum += cartItem.Sum.Amount;
        }

        TotalSum = new Money(totalSum, _items.FirstOrDefault()?.Price.Currency ?? Currency.RUB);
    }
    
    private Cart() { }
    
    public CartId Id { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public uint TotalProductsPositions { get; private set; }
    public Money TotalSum { get; private set; } = Zero;
    
    public DateTimeOffset CreatedAt { get; private set; }

    public uint RowVersion { get; private set; }
    
    private List<CartItem> _items = new();

    private static readonly Money Zero = new(0, Currency.RUB);
}

public class InvalidQuantityException : DomainException
{
    public InvalidQuantityException() : base("Quantity should be greater than 0")
    {
    }
}

public class ProductAlreadyInCartException : DomainException
{
    public ProductAlreadyInCartException() : base("Product already in cart")
    {
    }
}