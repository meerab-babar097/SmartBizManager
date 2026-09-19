using Microsoft.AspNetCore.Mvc.Rendering;
using SmartBizManager.Models;

namespace SmartBizManager.ViewModels;

public class ProductIndexViewModel
{
    public List<Product> Products { get; set; } = new();
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Stock { get; set; }   // "low" | "out" | null
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

    public int TotalCount { get; set; }
    public decimal InventoryValue { get; set; }
}