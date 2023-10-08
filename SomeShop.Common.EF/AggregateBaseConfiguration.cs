using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SomeShop.Common.Domain;

namespace SomeShop.Common.EF;

public abstract class AggregateBaseConfiguration<TAggregate> :
    IEntityTypeConfiguration<TAggregate>
    where TAggregate : class, IAggregate
{
    public void Configure(EntityTypeBuilder<TAggregate> builder)
    {
        ConfigureAggregate(builder);
        builder.Ignore(x => x.Events);
    }

    public abstract void ConfigureAggregate(EntityTypeBuilder<TAggregate> builder);
}
