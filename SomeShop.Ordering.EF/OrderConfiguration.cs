using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.EF;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.EF;

public class OrderConfiguration : AggregateBaseConfiguration<Order>
{
    public override void ConfigureAggregate(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, value => new OrderId(value));
        builder.Property(x => x.CartId)
            .HasConversion(x => x.Value, value => new CartId(value));
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .IsRequired();
        builder.OwnsOne(x => x.TotalSum);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, value => new OrderItemId(value));
        builder.Property(x => x.OrderId)
            .HasConversion(x => x.Value, value => new OrderId(value));
        builder.Property(x => x.ProductId)
            .HasConversion(x => x.Value, value => new ProductId(value));
        builder.OwnsOne(x => x.Price);
        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, value => new Quantity(value));
    }
}