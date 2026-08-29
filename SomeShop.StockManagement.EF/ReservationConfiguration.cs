using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.EF;
using SomeShop.StockManagement.Domain;

namespace SomeShop.StockManagement.EF;

public class ReservationConfiguration : AggregateBaseConfiguration<Reservation>
{
    public override void ConfigureAggregate(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.OrderId)
            .HasConversion(x => x.Value, value => new OrderId(value));
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .IsRequired();
    }
}

public class ReservationItemConfiguration : IEntityTypeConfiguration<ReservationItem>
{
    public void Configure(EntityTypeBuilder<ReservationItem> builder)
    {
        builder.ToTable("reservation_items");
        // A reservation holds at most one line per product, so the pair is the natural key.
        builder.HasKey(x => new { x.OrderId, x.ProductId });
        builder.Property(x => x.OrderId)
            .HasConversion(x => x.Value, value => new OrderId(value));
        builder.Property(x => x.ProductId)
            .HasConversion(x => x.Value, value => new ProductId(value));
    }
}
