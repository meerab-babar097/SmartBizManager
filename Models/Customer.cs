using System.ComponentModel.DataAnnotations;

namespace SmartBizManager.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone]
    public string? Phone { get; set; }

    [StringLength(120)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}