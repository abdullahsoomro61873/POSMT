using Microsoft.EntityFrameworkCore;
using POSMT.API.Models;

namespace POSMT.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoleName).IsUnique();
            e.HasData(
                new Role { RoleId = 1, RoleName = "Manager" },
                new Role { RoleId = 2, RoleName = "Cashier" });
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Role).WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Terminal>(e =>
        {
            e.Property(x => x.TerminalName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.TerminalName).IsUnique();
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.CategoryName).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Barcode).HasMaxLength(50);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasIndex(x => x.Barcode).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Products_Price", "[Price] >= 0"));
            e.HasOne(x => x.Category).WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Inventory>(e =>
        {
            e.Property(x => x.RowVersion).IsRowVersion();
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Inventory_Quantity", "[Quantity] >= 0");
                t.HasCheckConstraint("CK_Inventory_ReorderLevel", "[ReorderLevel] >= 0");
            });
            e.HasOne(x => x.Product).WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.SaleDate).HasDefaultValueSql("GETUTCDATE()");
            e.ToTable(t => t.HasCheckConstraint("CK_Sales_Amounts",
                "[Subtotal] >= 0 AND [Discount] >= 0 AND [Discount] <= [Subtotal] AND [TotalAmount] >= 0"));
            e.HasOne(x => x.Terminal).WithMany(t => t.Sales)
                .HasForeignKey(x => x.TerminalId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Cashier).WithMany(u => u.Sales)
                .HasForeignKey(x => x.CashierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleItem>(e =>
        {
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.LineTotal).HasPrecision(18, 2);
            e.ToTable(t => t.HasCheckConstraint("CK_SaleItems_Quantity", "[Quantity] > 0"));
            e.HasOne(x => x.Sale).WithMany(s => s.Items)
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(p => p.SaleItems)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.PaymentMethod).HasMaxLength(20).IsRequired();
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Sale).WithOne(s => s.Payment)
                .HasForeignKey<Payment>(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
