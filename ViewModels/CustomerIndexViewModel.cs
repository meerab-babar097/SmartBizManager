using SmartBizManager.Models;

namespace SmartBizManager.ViewModels;

public class CustomerRow
{
    public Customer Customer { get; set; } = null!;
    public decimal OutstandingAmount { get; set; }
    public int TotalOrders { get; set; }
}

public class CustomerIndexViewModel
{
    public List<CustomerRow> Rows { get; set; } = new();
    public string? Search { get; set; }
}