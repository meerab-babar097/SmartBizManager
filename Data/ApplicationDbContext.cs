using Microsoft.EntityFrameworkCore;
using SmartBizManager.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SmartBizManager.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Two products must never share an SKU. Enforced by the DATABASE,
        // not just by C# - that is what makes it trustworthy.
        b.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        b.Entity<Sale>().HasIndex(s => s.InvoiceNumber).IsUnique();
        b.Entity<Sale>().HasIndex(s => s.SaleDate);

        // Restrict = you cannot delete a category that still has products,
        // or a customer/product referenced by a sale. History stays intact.
        b.Entity<Product>()
            .HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Sale>()
            .HasOne(s => s.Customer).WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<SaleItem>()
            .HasOne(i => i.Product).WithMany(p => p.SaleItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade = deleting a sale deletes its lines. A line cannot exist alone.
        b.Entity<SaleItem>()
            .HasOne(i => i.Sale).WithMany(s => s.Items)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}