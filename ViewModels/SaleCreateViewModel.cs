using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartBizManager.ViewModels;

public class SaleLineInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a product for every row.")]
    public int ProductId { get; set; }

    [Range(1, 100_000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}

public class SaleCreateViewModel
{
    public int? CustomerId { get; set; }

    [Range(0, 10_000_000)]
    public decimal Discount { get; set; }

    [Range(0, 10_000_000)]
    [Display(Name = "Amount paid")]
    public decimal AmountPaid { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public List<SaleLineInput> Items { get; set; } = new();

    // Filled by the controller for the dropdowns/JS - never posted back meaningfully.
    public IEnumerable<SelectListItem> Customers { get; set; } = new List<SelectListItem>();
    public string ProductsJson { get; set; } = "[]";
}