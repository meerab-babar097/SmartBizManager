using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.ViewModels;

namespace SmartBizManager.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var toDate = (to ?? DateTime.Today).Date;
        var fromDate = (from ?? new DateTime(toDate.Year, toDate.Month, 1)).Date;
        var toExclusive = toDate.AddDays(1); // makes the "To" day fully inclusive

        var sales = await _db.Sales
            .Include(s => s.Items)
            .Where(s => s.SaleDate >= fromDate && s.SaleDate < toExclusive)
            .AsNoTracking()
            .ToListAsync();

        var expenses = await _db.Expenses
            .Where(e => e.Date >= fromDate && e.Date < toExclusive)
            .AsNoTracking()
            .ToListAsync();

        var revenue = sales.Sum(s => s.Total);
        // COGS = what the sold goods actually cost us, using the UnitCost snapshot
        // stored on each SaleItem at the moment it was sold - not today's purchase price.
        var cogs = sales.SelectMany(s => s.Items).Sum(i => i.Quantity * i.UnitCost);
        var expenseTotal = expenses.Sum(e => e.Amount);

        var vm = new ReportsViewModel
        {
            From = fromDate,
            To = toDate,
            SalesCount = sales.Count,
            TotalSalesRevenue = revenue,
            TotalCostOfGoodsSold = cogs,
            TotalExpenses = expenseTotal,
            EstimatedProfit = revenue - cogs - expenseTotal,

            ExpensesByCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseCategoryTotal { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList(),

            TopProducts = sales.SelectMany(s => s.Items)
                .GroupBy(i => i.ProductName)
                .Select(g => new ProductSalesSummary
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.LineTotal)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToList(),

            LowStockProducts = await _db.Products
                .Where(p => p.Quantity <= p.LowStockThreshold)
                .OrderBy(p => p.Quantity)
                .AsNoTracking()
                .ToListAsync()
        };

        return View(vm);
    }
}