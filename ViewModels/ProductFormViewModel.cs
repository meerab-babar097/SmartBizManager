using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartBizManager.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(120)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(40)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Range(0, 10_000_000, ErrorMessage = "Purchase price cannot be negative.")]
    [Display(Name = "Purchase price (Rs)")]
    public decimal PurchasePrice { get; set; }

    [Range(0, 10_000_000, ErrorMessage = "Selling price cannot be negative.")]
    [Display(Name = "Selling price (Rs)")]
    public decimal SellingPrice { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }

    [Range(0, 100_000)]
    [Display(Name = "Low-stock threshold")]
    public int LowStockThreshold { get; set; } = 5;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    // Populated by the controller, never posted back by the browser.
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}