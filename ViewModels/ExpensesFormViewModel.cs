using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartBizManager.Models;

namespace SmartBizManager.ViewModels;

public class ExpenseFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

    [Range(0.01, 10_000_000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(400)]
    public string? Description { get; set; }

    // Filled by the controller so the dropdown has friendly labels.
    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = new List<SelectListItem>();
}