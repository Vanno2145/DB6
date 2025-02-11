using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        using (AppDbContext db = new AppDbContext())
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var product1 = new Product { Name = "Laptop", Price = 1200 };
            var product2 = new Product { Name = "Smartphone", Price = 800 };
            db.Products.AddRange(product1, product2);
            db.SaveChanges();

            Console.WriteLine("Products:");
            foreach (var product in db.Products.ToList())
            {
                Console.WriteLine($"Id: {product.Id}, Name: {product.Name}, Price: {product.Price}");
            }

            var order = new Order { Date = DateTime.Now };
            db.Orders.Add(order);
            db.SaveChanges();

            order.Products.Add(product1);
            order.Products.Add(product2);
            db.SaveChanges();

            Console.WriteLine("Orders:");
            foreach (var o in db.Orders.Include(o => o.Products))
            {
                Console.WriteLine($"Order Id: {o.Id}, Date: {o.Date}");
                foreach (var p in o.Products)
                {
                    Console.WriteLine($" - Product: {p.Name}, Price: {p.Price}");
                }
            }
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int StockQuantity { get; set; } = 0;
    public string Description { get; set; }
    public List<Order> Orders { get; set; } = new();
    public string TemporaryData { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<Product> Products { get; set; } = new();
}

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=testdb;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(100);
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Price).HasColumnType("int");
            entity.Property(p => p.StockQuantity).HasDefaultValue(0);
            entity.Property(p => p.Description).IsRequired(false);
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Ignore(p => p.TemporaryData);
            entity.ToTable("StoreProducts");
            entity.HasCheckConstraint("CK_Price_NonNegative", "[Price] >= 0");
        });
    }
}
