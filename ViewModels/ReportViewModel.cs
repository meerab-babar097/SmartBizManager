namespace SmartBizManager.ViewModels;

public class ExpenseCategoryTotal
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class ProductSalesSummary
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class ReportsViewModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public int SalesCount { get; set; }
    public decimal TotalSalesRevenue { get; set; }
    public decimal TotalCostOfGoodsSold { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal EstimatedProfit { get; set; }

    public List<ExpenseCategoryTotal> ExpensesByCategory { get; set; } = new();
    public List<ProductSalesSummary> TopProducts { get; set; } = new();
    public List<Models.Product> LowStockProducts { get; set; } = new();
}