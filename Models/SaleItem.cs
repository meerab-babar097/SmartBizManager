using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBizManager.Models;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Snapshots. The product's name/price may change later; an invoice must not.
    [Required, StringLength(120)]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Precision(18, 2)] public decimal UnitPrice { get; set; }   // what we sold it for
    [Precision(18, 2)] public decimal UnitCost { get; set; }    // what it cost us -> profit reports

    [NotMapped] public decimal LineTotal => Quantity * UnitPrice;
}