using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBizManager.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(40)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Precision(18,2) stops SQL Server from silently using a rounded default for money.
    [Precision(18, 2)]
    [Display(Name = "Purchase price")]
    public decimal PurchasePrice { get; set; }

    [Precision(18, 2)]
    [Display(Name = "Selling price")]
    public decimal SellingPrice { get; set; }

    public int Quantity { get; set; }

    [Display(Name = "Low-stock threshold")]
    public int LowStockThreshold { get; set; } = 5;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    // [NotMapped] = computed in C#, no database column.
    // Important: you CANNOT use these inside a LINQ query that hits the database.
    [NotMapped] public bool IsOutOfStock => Quantity <= 0;
    [NotMapped] public bool IsLowStock => Quantity > 0 && Quantity <= LowStockThreshold;
    [NotMapped] public decimal ProfitPerUnit => SellingPrice - PurchasePrice;
}
