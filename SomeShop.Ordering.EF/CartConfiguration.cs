using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.EF;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.EF;

public class CartConfiguration : AggregateBaseConfiguration<Cart>
{
    public override void ConfigureAggregate(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, value => new CartId(value));
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.CartId)
            .IsRequired();
        builder.OwnsOne(x => x.TotalSum);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, value => new CartItemId(value));
        builder.Property(x => x.CartId)
            .HasConversion(x => x.Value, value => new CartId(value));
        builder.Property(x => x.ProductId)
            .HasConversion(x => x.Value, value => new ProductId(value));
        builder.OwnsOne(x => x.Price);
        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, value => new Quantity(value));
        builder.Ignore(x => x.Sum);
    }
}