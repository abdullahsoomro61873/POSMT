using Microsoft.EntityFrameworkCore;
using POSMT.API.Data;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;

namespace POSMT.API.Services;

public class SaleService : ISaleService
{
    private const int MaxRetries = 5;

    private readonly AppDbContext _db;

    public SaleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<SaleDto>> CreateAsync(
        int cashierId,
        SaleCreateRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return ServiceResult<SaleDto>.Invalid("A sale must contain at least one item.");
        }

        var paymentMethod = request.PaymentMethod?.Trim();

        if (string.Equals(paymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            paymentMethod = "Cash";
        }
        else if (string.Equals(paymentMethod, "Card", StringComparison.OrdinalIgnoreCase))
        {
            paymentMethod = "Card";
        }
        else
        {
            return ServiceResult<SaleDto>.Invalid("Payment method must be Cash or Card.");
        }

        if (request.Items.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
        {
            return ServiceResult<SaleDto>.Invalid("Product IDs and quantities must be greater than zero.");
        }

        if (request.Discount < 0 || request.AmountPaid < 0)
        {
            return ServiceResult<SaleDto>.Invalid("Discount and payment amount cannot be negative.");
        }

        var requestedItems = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            })
            .ToList();

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var terminal = await _db.Terminals
                    .FirstOrDefaultAsync(t => t.TerminalId == request.TerminalId);

                if (terminal is null || terminal.Status != "Active")
                {
                    return ServiceResult<SaleDto>.Invalid("Terminal does not exist or is inactive.");
                }

                var cashier = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == cashierId && u.IsActive);
                if (cashier is null)
                {
                    return ServiceResult<SaleDto>.Unauthorized("Cashier account is not active or does not exist.");
                }

                if (cashier.Role.RoleName != RoleNames.Cashier)
                {
                    return ServiceResult<SaleDto>.Unauthorized(
                        "Only cashiers can create sales.");
                }

                if (cashier.TerminalId != request.TerminalId)
                {
                    return ServiceResult<SaleDto>.Unauthorized(
                        "Cashier is not assigned to this terminal.");
                }

                var productIds = requestedItems.Select(i => i.ProductId).ToList();

                var products = await _db.Products
                    .Include(p => p.Inventory)
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    return ServiceResult<SaleDto>.Invalid("One or more products do not exist.");
                }

                if (products.Any(p => !p.IsActive))
                {
                    return ServiceResult<SaleDto>.Invalid("One or more products are inactive.");
                }

                var productLookup = products.ToDictionary(p => p.ProductId);

                decimal subtotal = 0;

                foreach (var item in requestedItems)
                {
                    var product = productLookup[item.ProductId];

                    if (product.Inventory is null)
                    {
                        return ServiceResult<SaleDto>.Conflict(
                            $"Inventory record is missing for product {product.Name}.");
                    }

                    if (product.Inventory.Quantity < item.Quantity)
                    {
                        return ServiceResult<SaleDto>.Conflict(
                            $"Insufficient stock for {product.Name}. Available: {product.Inventory.Quantity}.");
                    }

                    subtotal += product.Price * item.Quantity;
                }

                if (request.Discount > subtotal)
                {
                    return ServiceResult<SaleDto>.Invalid("Discount cannot exceed the subtotal.");
                }

                var totalAmount = subtotal - request.Discount;

                if (paymentMethod == "Cash" && request.AmountPaid < totalAmount)
                {
                    return ServiceResult<SaleDto>.Invalid("Cash payment is insufficient.");
                }

                if (paymentMethod == "Card" && request.AmountPaid != totalAmount)
                {
                    return ServiceResult<SaleDto>.Invalid("Card payment amount must equal the sale total.");
                }

                var sale = new Sale
                {
                    TerminalId = terminal.TerminalId,
                    CashierId = cashier.UserId,
                    SaleDate = DateTime.UtcNow,
                    Subtotal = subtotal,
                    Discount = request.Discount,
                    TotalAmount = totalAmount,
                    Status = "Completed"
                };

                foreach (var item in requestedItems)
                {
                    var product = productLookup[item.ProductId];

                    sale.Items.Add(new SaleItem
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = product.Price * item.Quantity
                    });

                    product.Inventory!.Quantity -= item.Quantity;
                }

                sale.Payment = new Payment
                {
                    PaymentMethod = paymentMethod,
                    Amount = request.AmountPaid,
                    PaymentStatus = "Completed",
                    PaidAt = DateTime.UtcNow
                };

                _db.Sales.Add(sale);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await LoadSaleDtoAsync(sale.SaleId);

                return result is null
                    ? ServiceResult<SaleDto>.NotFound("Sale was created but could not be loaded.")
                    : ServiceResult<SaleDto>.Ok(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _db.ChangeTracker.Clear();

                if (attempt == MaxRetries)
                {
                    return ServiceResult<SaleDto>.Conflict(
                        "Stock was changed by another terminal. Please try again.");
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                _db.ChangeTracker.Clear();
                throw;
            }
        }

        return ServiceResult<SaleDto>.Conflict(
            "Unable to complete the sale because stock is being updated.");
    }

    public async Task<ServiceResult<SaleDto>> GetByIdAsync(
        int id,
        int userId,
        bool isManager)
    {
        var sale = await LoadSaleDtoAsync(id);

        if (sale is null)
        {
            return ServiceResult<SaleDto>.NotFound("Sale not found.");
        }

        if (!isManager && sale.CashierId != userId)
        {
            return ServiceResult<SaleDto>.Unauthorized(
                "You are not authorized to view this sale.");
        }

        return ServiceResult<SaleDto>.Ok(sale);
    }

    private async Task<SaleDto?> LoadSaleDtoAsync(int saleId)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Include(s => s.Terminal)
            .Include(s => s.Cashier)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Payment)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);

        if (sale is null)
        {
            return null;
        }

        var items = sale.Items
            .Select(i => new SaleItemDto(
                i.ProductId,
                i.Product.Name,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal))
            .ToList();

        PaymentDto? payment = sale.Payment is null
            ? null
            : new PaymentDto(
                sale.Payment.PaymentMethod,
                sale.Payment.Amount,
                sale.Payment.PaidAt ?? DateTime.MinValue);

        var changeDue = sale.Payment?.PaymentMethod == "Cash"
            ? Math.Max(0, sale.Payment.Amount - sale.TotalAmount)
            : 0;

        return new SaleDto(
            sale.SaleId,
            sale.TerminalId,
            sale.Terminal.TerminalName,
            sale.CashierId,
            sale.Cashier.Name,
            sale.SaleDate,
            sale.Subtotal,
            sale.Discount,
            sale.TotalAmount,
            sale.Status,
            items,
            payment,
            changeDue);
    }
}


