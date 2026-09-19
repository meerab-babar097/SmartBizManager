using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.Models;
using SmartBizManager.ViewModels;
using System.Diagnostics;

namespace SmartBizManager.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Today;

        var vm = new DashboardViewModel
        {
            TotalProducts = await _db.Products.CountAsync(p => p.IsActive),
            TotalCustomers = await _db.Customers.CountAsync(),
            LowStockCount = await _db.Products.CountAsync(p => p.Quantity > 0 && p.Quantity <= p.LowStockThreshold),
            OutOfStockCount = await _db.Products.CountAsync(p => p.Quantity <= 0),
            InventoryValue = await _db.Products.SumAsync(p => p.Quantity * p.PurchasePrice),
            TotalSalesRevenue = await _db.Sales.SumAsync(s => (decimal?)s.Total) ?? 0,
            SalesToday = await _db.Sales.Where(s => s.SaleDate.Date == now).SumAsync(s => (decimal?)s.Total) ?? 0,
            LowStockProducts = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity <= p.LowStockThreshold)
                .OrderBy(p => p.Quantity)
                .Take(8)
                .AsNoTracking()
                .ToListAsync(),

            ExpensesThisMonth = await _db.Expenses
                .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                .SumAsync(e => e.Amount),

            // Still 0 until Milestone 3 gives you a Sales table with real rows - correct, not broken.
            TotalOutstanding = await _db.Sales.SumAsync(s => (decimal?)(s.Total - s.AmountPaid)) ?? 0
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}