using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.EF;
using SomeShop.StockManagement.Domain;

namespace SomeShop.StockManagement.EF;

public class StockItemConfiguration : AggregateBaseConfiguration<StockItem>
{
    public override void ConfigureAggregate(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.ProductId)
            .HasConversion(x => x.Value, value => new ProductId(value));
    }
}
