using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.Services;
using SmartBizManager.ViewModels;

namespace SmartBizManager.Controllers;

public class SalesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SaleService _saleService;

    public SalesController(ApplicationDbContext db, SaleService saleService)
    {
        _db = db;
        _saleService = saleService;
    }

    public async Task<IActionResult> Index()
    {
        var sales = await _db.Sales
            .Include(s => s.Customer)
            .OrderByDescending(s => s.SaleDate)
            .AsNoTracking()
            .ToListAsync();

        return View(sales);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new SaleCreateViewModel();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaleCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var result = await _saleService.CreateSaleAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not complete the sale.");
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["Success"] = $"Sale {result.Sale!.InvoiceNumber} recorded.";
        return RedirectToAction(nameof(Details), new { id = result.Sale.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound();
        return View(sale);
    }

    private async Task PopulateDropdownsAsync(SaleCreateViewModel vm)
    {
        vm.Customers = await _db.Customers
            .OrderBy(c => c.Name)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();

        // Sent to the browser as JSON so the JS can show live price/stock
        // per row and calculate totals without asking the server every keystroke.
        var products = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, price = p.SellingPrice, stock = p.Quantity })
            .ToListAsync();

        vm.ProductsJson = JsonSerializer.Serialize(products, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    public async Task<IActionResult> Invoice(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound();
        return View(sale);
    }
}