using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.Models;
using SmartBizManager.ViewModels;

namespace SmartBizManager.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    // Dependency Injection: we never write "new ApplicationDbContext()".
    // ASP.NET Core creates one per request and hands it to us.
    public ProductsController(ApplicationDbContext db) => _db = db;

    // GET /Products
    public async Task<IActionResult> Index(string? search, int? categoryId, string? stock)
    {
        // IQueryable = a query being BUILT. Nothing hits SQL Server until ToListAsync().
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        // Note: we repeat the comparison instead of using p.IsLowStock.
        // [NotMapped] properties cannot be translated into SQL.
        if (stock == "low")
            query = query.Where(p => p.Quantity > 0 && p.Quantity <= p.LowStockThreshold);
        else if (stock == "out")
            query = query.Where(p => p.Quantity <= 0);

        var products = await query
            .OrderBy(p => p.Name)
            .AsNoTracking()          // read-only list -> skip change tracking, faster
            .ToListAsync();

        var vm = new ProductIndexViewModel
        {
            Products = products,
            Search = search,
            CategoryId = categoryId,
            Stock = stock,
            Categories = await CategorySelectListAsync(),
            TotalCount = products.Count,
            InventoryValue = products.Sum(p => p.Quantity * p.PurchasePrice)
        };

        return View(vm);
    }

    // GET /Products/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();
        return View(product);
    }

    // GET /Products/Create
    public async Task<IActionResult> Create()
        => View(new ProductFormViewModel { Categories = await CategorySelectListAsync() });

    // POST /Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]   // blocks CSRF: another site cannot post this form for you
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        await ValidateBusinessRulesAsync(vm);

        if (!ModelState.IsValid)
        {
            vm.Categories = await CategorySelectListAsync();
            return View(vm);
        }

        var product = new Product
        {
            Name = vm.Name.Trim(),
            Sku = vm.Sku.Trim().ToUpperInvariant(),
            CategoryId = vm.CategoryId,
            PurchasePrice = vm.PurchasePrice,
            SellingPrice = vm.SellingPrice,
            Quantity = vm.Quantity,
            LowStockThreshold = vm.LowStockThreshold,
            IsActive = vm.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Product \"{product.Name}\" was added.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Products/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();

        return View(new ProductFormViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            CategoryId = p.CategoryId,
            PurchasePrice = p.PurchasePrice,
            SellingPrice = p.SellingPrice,
            Quantity = p.Quantity,
            LowStockThreshold = p.LowStockThreshold,
            IsActive = p.IsActive,
            Categories = await CategorySelectListAsync()
        });
    }

    // POST /Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        await ValidateBusinessRulesAsync(vm);

        if (!ModelState.IsValid)
        {
            vm.Categories = await CategorySelectListAsync();
            return View(vm);
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        product.Name = vm.Name.Trim();
        product.Sku = vm.Sku.Trim().ToUpperInvariant();
        product.CategoryId = vm.CategoryId;
        product.PurchasePrice = vm.PurchasePrice;
        product.SellingPrice = vm.SellingPrice;
        product.Quantity = vm.Quantity;
        product.LowStockThreshold = vm.LowStockThreshold;
        product.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Product \"{product.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Products/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();

        ViewBag.HasSales = await _db.SaleItems.AnyAsync(i => i.ProductId == id);
        return View(product);
    }

    // POST /Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        // BUSINESS RULE: a product that appears on a past invoice is never
        // deleted - that would corrupt history. We deactivate it instead.
        if (await _db.SaleItems.AnyAsync(i => i.ProductId == id))
        {
            product.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"\"{product.Name}\" has sales history, so it was deactivated instead of deleted.";
        }
        else
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Product \"{product.Name}\" was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------

    private async Task<IEnumerable<SelectListItem>> CategorySelectListAsync()
        => await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

    private async Task ValidateBusinessRulesAsync(ProductFormViewModel vm)
    {
        var sku = (vm.Sku ?? string.Empty).Trim().ToUpperInvariant();

        // Uniqueness must be checked server-side. The DB index is the real guard,
        // but this gives the user a friendly message instead of a crash.
        bool duplicate = await _db.Products
            .AnyAsync(p => p.Sku == sku && p.Id != vm.Id);

        if (duplicate)
            ModelState.AddModelError(nameof(vm.Sku), "Another product already uses this SKU.");

        if (vm.SellingPrice < vm.PurchasePrice)
            ModelState.AddModelError(nameof(vm.SellingPrice),
                "Selling price is below purchase price — this product would sell at a loss.");
    }
}