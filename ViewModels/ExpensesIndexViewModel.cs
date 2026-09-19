using SmartBizManager.Models;

namespace SmartBizManager.ViewModels;

public class ExpenseIndexViewModel
{
    public List<Expense> Expenses { get; set; } = new();
    public decimal TotalThisMonth { get; set; }
    public decimal TotalAllTime { get; set; }
    public string? Category { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}