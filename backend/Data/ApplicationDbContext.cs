using AdvancedOrderSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedOrderSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // AdminUser
        // =========================

        modelBuilder.Entity<AdminUser>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<AdminUser>()
            .Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<AdminUser>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        // =========================
        // Product
        // =========================

        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Product>()
           .Property(p => p.Name)
           .IsRequired()
           .HasMaxLength(150);

        modelBuilder.Entity<Product>()
            .Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        // =========================
        // Customer
        // =========================

        modelBuilder.Entity<Customer>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Customer>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Customer>()
            .Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // =========================
        // Order
        // =========================

        modelBuilder.Entity<Order>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<Order>()
            .Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(50);


        // Customer 1 -> Many Orders
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // OrderItem
        // =========================

        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => oi.Id);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.LineTotal)
            .HasPrecision(18, 2);

        // Order 1 -> Many OrderItems
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product 1 -> Many OrderItems
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // Seed Products
        // =========================

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                SKU = "LAP-001",
                UnitPrice = 150000.00m,
                Stock = 10,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 2,
                Name = "Mouse",
                SKU = "MOU-001",
                UnitPrice = 2500.00m,
                Stock = 25,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 3,
                Name = "Keyboard",
                SKU = "KEY-001",
                UnitPrice = 5000.00m,
                Stock = 20,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 4,
                Name = "Monitor",
                SKU = "MON-001",
                UnitPrice = 45000.00m,
                Stock = 8,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Product
            {
                Id = 5,
                Name = "Headphones",
                SKU = "HEA-001",
                UnitPrice = 7500.00m,
                Stock = 15,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );

        // =========================
        // Seed Customers
        // =========================

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                Name = "John Silva",
                Email = "john@example.com",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Customer
            {
                Id = 2,
                Name = "Kasun Perera",
                Email = "kasun@example.com",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Customer
            {
                Id = 3,
                Name = "Nimal Fernando",
                Email = "nimal@example.com",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}