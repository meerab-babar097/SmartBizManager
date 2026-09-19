using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SmartBizManager.Models;

public enum ExpenseCategory
{
    Rent = 0,
    Utilities = 1,
    Salaries = 2,
    Transport = 3,
    Marketing = 4,
    Internet = 5,
    Supplies = 6,
    Other = 7
}

public class Expense
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(400)]
    public string? Description { get; set; }
}