using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartBizManager.Models;

public class Sale
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string InvoiceNumber { get; set; } = string.Empty;

    // Nullable = walk-in customer with no record kept.
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.Now;

    [Precision(18, 2)] public decimal Subtotal { get; set; }
    [Precision(18, 2)] public decimal Discount { get; set; }
    [Precision(18, 2)] public decimal Total { get; set; }
    [Precision(18, 2)] public decimal AmountPaid { get; set; }

    [StringLength(400)] public string? Notes { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

    [NotMapped] public decimal Remaining => Total - AmountPaid;
    [NotMapped] public bool IsFullyPaid => Remaining <= 0;
}