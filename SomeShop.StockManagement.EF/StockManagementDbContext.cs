using Microsoft.EntityFrameworkCore;
using SomeShop.Common.EF;
using SomeShop.StockManagement.Domain;

#pragma warning disable CS8618

namespace SomeShop.StockManagement.EF;

public class StockManagementDbContext : BaseDbContext
{
    public StockManagementDbContext(DbContextOptions<StockManagementDbContext> options) : base(options) { }

    public DbSet<StockItem> StockItems { get; set; }

    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("stock_management");

        modelBuilder.ApplyConfiguration(new StockItemConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationItemConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
