using Microsoft.EntityFrameworkCore;

namespace SomeShop.Common.EF;

public class BaseDbContext : DbContext
{
    public BaseDbContext(DbContextOptions options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.UsePropertyConfigurators(
            new StringEnumConverterConfigurator(),
            new CurrencyEnumColumnTypeConfigurator(), 
            new RowVersionConfigurator(),
            new CreatedDateConfigurator());
    }
}