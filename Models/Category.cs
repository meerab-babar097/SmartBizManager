using System.ComponentModel.DataAnnotations;

namespace SmartBizManager.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;

    // Navigation property: lets you write category.Products instead of a manual join.
    public ICollection<Product> Products { get; set; } = new List<Product>();
}