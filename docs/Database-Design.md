# POSMT Database Design

## ER Diagram

```mermaid
erDiagram
    Roles ||--o{ Users : has
    Users ||--o{ Sales : processes
    Terminals ||--o{ Sales : handles
    Categories ||--o{ Products : contains
    Products ||--|| Inventory : has
    Products ||--o{ SaleItems : appears_in
    Sales ||--|{ SaleItems : contains
    Sales ||--|| Payments : paid_by

    Roles {
        int RoleId PK
        string RoleName
    }
    Users {
        int UserId PK
        string Name
        string Email
        string PasswordHash
        int RoleId FK
        bool IsActive
    }
    Terminals {
        int TerminalId PK
        string TerminalName
        string Status
    }
    Categories {
        int CategoryId PK
        string CategoryName
    }
    Products {
        int ProductId PK
        string Name
        string Barcode
        decimal Price
        int CategoryId FK
        bool IsActive
    }
    Inventory {
        int InventoryId PK
        int ProductId FK
        int Quantity
        int ReorderLevel
        binary RowVersion
    }
    Sales {
        int SaleId PK
        int TerminalId FK
        int CashierId FK
        datetime SaleDate
        decimal Subtotal
        decimal Discount
        decimal TotalAmount
        string Status
    }
    SaleItems {
        int SaleItemId PK
        int SaleId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
        decimal LineTotal
    }
    Payments {
        int PaymentId PK
        int SaleId FK
        string PaymentMethod
        decimal Amount
        datetime PaidAt
    }
```

## Tables

| Table | Notes |
|---|---|
| Roles | Manager, Cashier. Seeded at startup. |
| Users | Email is unique. Stores password hash only. CashierId in Sales points here. |
| Terminals | Status is Active or Inactive. TerminalName is unique. |
| Categories | CategoryName is unique. |
| Products | Barcode is unique. Price is never negative. IsActive replaces deletion. |
| Inventory | One row per product. RowVersion is used for concurrency control. |
| Sales | One row per bill. Status is Completed or Cancelled. |
| SaleItems | UnitPrice is copied from Products at sale time. |
| Payments | One payment per sale in the MVP. PaymentMethod is Cash or Card. |

## Key Constraints

- Users.Email: unique
- Products.Barcode: unique
- Terminals.TerminalName: unique
- Inventory.ProductId: unique (one-to-one with Products)
- Inventory.Quantity: must be 0 or more
- Products.Price: must be 0 or more
- SaleItems.Quantity: must be greater than 0
- Payments.SaleId: unique (one payment per sale)

## Delete Rules

- Category with products: delete is blocked.
- Product with sales: deactivate only.
- Sale: never deleted, only Cancelled.

## Concurrency Strategy

Inventory has a RowVersion column. When two terminals try to update the same stock at the same time, Entity Framework Core detects the conflict. The second sale is rejected or retried, so stock never goes below zero.