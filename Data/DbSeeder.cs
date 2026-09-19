using SmartBizManager.Models;

namespace SmartBizManager.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Categories.Any()) return; // never seed twice

        var electronics = new Category { Name = "Electronics" };
        var grocery = new Category { Name = "Grocery" };
        var stationery = new Category { Name = "Stationery" };
        var clothing = new Category { Name = "Clothing" };

        db.Categories.AddRange(electronics, grocery, stationery, clothing);
        db.SaveChanges();

        db.Products.AddRange(
            new Product
            {
                Name = "USB-C Cable 1m",
                Sku = "ELC-001",
                CategoryId = electronics.Id,
                PurchasePrice = 250,
                SellingPrice = 450,
                Quantity = 40,
                LowStockThreshold = 10
            },
            new Product
            {
                Name = "Wireless Mouse",
                Sku = "ELC-002",
                CategoryId = electronics.Id,
                PurchasePrice = 1100,
                SellingPrice = 1750,
                Quantity = 4,
                LowStockThreshold = 5
            },
            new Product
            {
                Name = "Basmati Rice 5kg",
                Sku = "GRC-001",
                CategoryId = grocery.Id,
                PurchasePrice = 1800,
                SellingPrice = 2200,
                Quantity = 0,
                LowStockThreshold = 6
            },
            new Product
            {
                Name = "A4 Notebook",
                Sku = "STN-001",
                CategoryId = stationery.Id,
                PurchasePrice = 120,
                SellingPrice = 200,
                Quantity = 85,
                LowStockThreshold = 15
            },
            new Product
            {
                Name = "Cotton T-Shirt",
                Sku = "CLT-001",
                CategoryId = clothing.Id,
                PurchasePrice = 700,
                SellingPrice = 1200,
                Quantity = 22,
                LowStockThreshold = 8
            }
        );

        db.Customers.AddRange(
            new Customer { Name = "Ahmed Traders", Phone = "0300-1234567", Address = "Anarkali, Lahore" },
            new Customer { Name = "Sana Khan", Phone = "0321-7654321", Email = "sana@example.com" }
        );

        db.SaveChanges();
    }
}