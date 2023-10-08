using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.EF;

#pragma warning disable CS8618

namespace SomeShop.Catalog.EF;

public class CatalogDbContext : BaseDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");

            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasColumnType("varchar(50)");
            e.Property(x => x.Id).HasConversion(x => x.Value, v => new ProductId(v));
            e.OwnsOne(x => x.Price);
        });
    }
}