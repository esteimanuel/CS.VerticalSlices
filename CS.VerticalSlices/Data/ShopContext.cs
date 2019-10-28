using CS.VerticalSlices.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.VerticalSlices.Data
{
    public class ShopContext : DbContext
    {
        public DbSet<SalesOrder> SalesOrders { get; set; } 
        public DbSet<Product> Products { get; set; } 

        public ShopContext() : base() { }

        public ShopContext(DbContextOptions<ShopContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<SalesOrder>(ConfigureSalesOrder);
            modelBuilder.Entity<Product>(ConfigureProduct);
        }

        private static void ConfigureSalesOrder(EntityTypeBuilder<SalesOrder> builder)
        {
            builder.ToTable(nameof(SalesOrder));
            builder.HasKey(order => order.Id);
            builder.OwnsMany(order => order.Lines, ob =>
            {
                ob.ToTable("SalesOrderLine");
                ob.WithOwner().HasForeignKey(line => line.OrderId);
                ob.HasOne(line => line.Product).WithMany().HasForeignKey(line => line.ProductId);
                ob.HasKey(line => new { line.OrderId, line.Number });
            });
        }

        private static void ConfigureProduct(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable(nameof(Product));
            builder.HasKey(product => product.Id);
            builder.HasAlternateKey(product => product.Code);
        }
    }
}
