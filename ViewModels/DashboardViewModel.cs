using SmartBizManager.Models;

namespace SmartBizManager.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal InventoryValue { get; set; }
    public decimal TotalSalesRevenue { get; set; }
    public decimal SalesToday { get; set; }
    public List<Product> LowStockProducts { get; set; } = new();

    // New this milestone
    public decimal ExpensesThisMonth { get; set; }
    public decimal TotalOutstanding { get; set; }
}