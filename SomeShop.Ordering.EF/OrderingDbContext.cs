using Microsoft.EntityFrameworkCore;
using SomeShop.Common.EF;
using SomeShop.Ordering.Domain;
using Product = SomeShop.Catalog.EF.Product;

#pragma warning disable CS8618

namespace SomeShop.Ordering.EF;

public class OrderingDbContext : BaseDbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) {}
    
    public DbSet<Cart> Carts { get; set; }
    
    public DbSet<CartItem> CartItems { get; set; }
    
    public DbSet<Order> Orders { get; set; }
    
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ordering");

        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}