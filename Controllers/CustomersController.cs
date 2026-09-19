using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.Models;
using SmartBizManager.ViewModels;

namespace SmartBizManager.Controllers;

public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;
    public CustomersController(ApplicationDbContext db) => _db = db;

    // GET /Customers
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term) ||
                                      (c.Phone != null && c.Phone.Contains(term)));
        }

        var customers = await query.OrderBy(c => c.Name).AsNoTracking().ToListAsync();

        // Sale table is empty until Milestone 3, so this returns 0 for everyone right now.
        // That's correct behavior, not a bug - it'll light up automatically once sales exist.
        var rows = new List<CustomerRow>();
        foreach (var c in customers)
        {
            var sales = await _db.Sales.Where(s => s.CustomerId == c.Id).ToListAsync();
            rows.Add(new CustomerRow
            {
                Customer = c,
                TotalOrders = sales.Count,
                OutstandingAmount = sales.Sum(s => s.Total - s.AmountPaid)
            });
        }

        return View(new CustomerIndexViewModel { Rows = rows, Search = search });
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return NotFound();

        ViewBag.Sales = await _db.Sales
            .Where(s => s.CustomerId == id)
            .OrderByDescending(s => s.SaleDate)
            .AsNoTracking()
            .ToListAsync();

        return View(customer);
    }

    public IActionResult Create() => View(new CustomerFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _db.Customers.Add(new Customer
        {
            Name = vm.Name.Trim(),
            Phone = vm.Phone?.Trim(),
            Email = vm.Email?.Trim(),
            Address = vm.Address?.Trim(),
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Customer \"{vm.Name}\" was added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();

        return View(new CustomerFormViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return NotFound();

        customer.Name = vm.Name.Trim();
        customer.Phone = vm.Phone?.Trim();
        customer.Email = vm.Email?.Trim();
        customer.Address = vm.Address?.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Customer \"{customer.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return NotFound();

        ViewBag.HasSales = await _db.Sales.AnyAsync(s => s.CustomerId == id);
        return View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return NotFound();

        // BUSINESS RULE: a customer with sales history is never deleted -
        // that would corrupt past invoices. This mirrors the Product rule.
        if (await _db.Sales.AnyAsync(s => s.CustomerId == id))
        {
            TempData["Error"] = $"\"{customer.Name}\" has sales on record and can't be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Customer \"{customer.Name}\" was deleted.";
        return RedirectToAction(nameof(Index));
    }
}