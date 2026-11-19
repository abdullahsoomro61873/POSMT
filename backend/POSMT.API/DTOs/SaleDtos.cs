using System.ComponentModel.DataAnnotations;

namespace POSMT.API.DTOs;

public class SaleItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100000)]
    public int Quantity { get; set; }
}

public class SaleCreateRequest
{
    [Range(1, int.MaxValue)]
    public int TerminalId { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    public List<SaleItemRequest> Items { get; set; } = new();

    [Range(0, 100000000)]
    public decimal Discount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal AmountPaid { get; set; }
}

public record SaleItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record PaymentDto(string PaymentMethod, decimal Amount, DateTime PaidAt);

public record SaleDto(
    int SaleId,
    int TerminalId,
    string TerminalName,
    int CashierId,
    string CashierName,
    DateTime SaleDate,
    decimal Subtotal,
    decimal Discount,
    decimal TotalAmount,
    string Status,
    List<SaleItemDto> Items,
    PaymentDto? Payment,
    decimal ChangeDue);
