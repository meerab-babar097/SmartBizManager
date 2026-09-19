using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.Models;
using SmartBizManager.ViewModels;

namespace SmartBizManager.Controllers;

public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _db;
    public ExpensesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? category, DateTime? from, DateTime? to)
    {
        var query = _db.Expenses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<ExpenseCategory>(category, out var cat))
        {
            query = query.Where(e => e.Category == cat);
        }
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);

        var expenses = await query.OrderByDescending(e => e.Date).AsNoTracking().ToListAsync();

        var now = DateTime.Today;
        var monthTotal = await _db.Expenses
            .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
            .SumAsync(e => e.Amount);

        return View(new ExpenseIndexViewModel
        {
            Expenses = expenses,
            TotalThisMonth = monthTotal,
            TotalAllTime = await _db.Expenses.SumAsync(e => e.Amount),
            Category = category,
            From = from,
            To = to
        });
    }

    public IActionResult Create() =>
        View(new ExpenseFormViewModel { CategoryOptions = CategoryOptions() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = CategoryOptions();
            return View(vm);
        }

        _db.Expenses.Add(new Expense
        {
            Title = vm.Title.Trim(),
            Category = vm.Category,
            Amount = vm.Amount,
            Date = vm.Date,
            Description = vm.Description?.Trim()
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Expense \"{vm.Title}\" was recorded.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.Expenses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound();

        return View(new ExpenseFormViewModel
        {
            Id = e.Id,
            Title = e.Title,
            Category = e.Category,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description,
            CategoryOptions = CategoryOptions()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = CategoryOptions();
            return View(vm);
        }

        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null) return NotFound();

        expense.Title = vm.Title.Trim();
        expense.Category = vm.Category;
        expense.Amount = vm.Amount;
        expense.Date = vm.Date;
        expense.Description = vm.Description?.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Expense \"{expense.Title}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null) return NotFound();
        return View(expense);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null) return NotFound();

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Expense \"{expense.Title}\" was deleted.";
        return RedirectToAction(nameof(Index));
    }

    // Expenses have no delete restriction - unlike Products/Customers, an
    // expense never gets referenced by another table, so removing it is safe.

    private static IEnumerable<SelectListItem> CategoryOptions() =>
        Enum.GetValues<ExpenseCategory>()
            .Select(c => new SelectListItem { Value = c.ToString(), Text = c.ToString() });
}