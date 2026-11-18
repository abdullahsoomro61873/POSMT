namespace POSMT.API.Models;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // A cashier can be assigned to one terminal.
    // Managers do not need a terminal assignment.
    public int? TerminalId { get; set; }
    public Terminal? Terminal { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class Terminal
{
    public int TerminalId { get; set; }
    public string TerminalName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public Inventory? Inventory { get; set; }

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public class Sale
{
    public int SaleId { get; set; }

    public int TerminalId { get; set; }
    public Terminal Terminal { get; set; } = null!;

    public int CashierId { get; set; }
    public User Cashier { get; set; } = null!;

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }

    // A sale is completed only after successful payment.
    public string Status { get; set; } = "Pending";

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public Payment? Payment { get; set; }
}

public class SaleItem
{
    public int SaleItemId { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    // Historical product price at the time of sale.
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public string PaymentStatus { get; set; } = "Pending";

    // Set only when payment is completed.
    public DateTime? PaidAt { get; set; }
}
