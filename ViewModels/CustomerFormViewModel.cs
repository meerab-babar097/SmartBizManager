using System.ComponentModel.DataAnnotations;

namespace SmartBizManager.ViewModels;

public class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone(ErrorMessage = "Enter a valid phone number.")]
    public string? Phone { get; set; }

    [StringLength(120)]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }
}