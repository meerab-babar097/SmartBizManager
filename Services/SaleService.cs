using Microsoft.EntityFrameworkCore;
using SmartBizManager.Data;
using SmartBizManager.Models;
using SmartBizManager.ViewModels;

namespace SmartBizManager.Services;

public class SaleResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Sale? Sale { get; set; }
}

public class SaleService
{
    private readonly ApplicationDbContext _db;
    public SaleService(ApplicationDbContext db) => _db = db;

    public async Task<SaleResult> CreateSaleAsync(SaleCreateViewModel vm)
    {
        var lines = vm.Items.Where(i => i.Quantity > 0).ToList();
        if (!lines.Any())
            return new SaleResult { Success = false, Error = "Add at least one product with a quantity greater than zero." };

        // Merge duplicate rows: if the same product was added on two lines,
        // check the combined quantity against stock, not each line separately.
        var requested = lines.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var products = await _db.Products
            .Where(p => requested.Keys.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var (productId, qty) in requested)
        {
            if (!products.TryGetValue(productId, out var product))
                return new SaleResult { Success = false, Error = "A selected product no longer exists." };

            if (qty > product.Quantity)
                return new SaleResult { Success = false, Error = $"Not enough stock for \"{product.Name}\" — only {product.Quantity} left." };
        }

        // Transaction: either every step below succeeds, or NONE of it is saved.
        // Without this, a crash after decrementing stock but before saving the
        // sale would silently lose inventory with no record of why.
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var sale = new Sale
            {
                CustomerId = vm.CustomerId,
                SaleDate = DateTime.Now,
                Discount = vm.Discount,
                AmountPaid = vm.AmountPaid,
                Notes = vm.Notes?.Trim(),
                InvoiceNumber = "PENDING" // real number assigned below, once we have an Id
            };

            decimal subtotal = 0;
            foreach (var item in lines)
            {
                var product = products[item.ProductId];

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,   // snapshot - survives future renames
                    Quantity = item.Quantity,
                    UnitPrice = product.SellingPrice,  // snapshot - survives future price changes
                    UnitCost = product.PurchasePrice
                });

                subtotal += item.Quantity * product.SellingPrice;
                product.Quantity -= item.Quantity;
            }

            if (vm.Discount > subtotal)
                return Rollback(transaction, "Discount cannot be greater than the subtotal.");

            sale.Subtotal = subtotal;
            sale.Total = subtotal - vm.Discount;

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync();          // sale.Id is now populated by the database

            sale.InvoiceNumber = $"INV-{sale.Id:D5}";
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return new SaleResult { Success = true, Sale = sale };
        }
        catch
        {
            await transaction.RollbackAsync();
            return new SaleResult { Success = false, Error = "Something went wrong while saving the sale. Nothing was changed." };
        }
    }

    private static SaleResult Rollback(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction t, string error)
    {
        t.RollbackAsync().GetAwaiter().GetResult();
        return new SaleResult { Success = false, Error = error };
    }
}