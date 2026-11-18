# POSMT Requirements

## 1. Overview

POSMT is a centralized Point of Sale system. Multiple cashier terminals share one product catalogue, one inventory and one database.

## 2. User Roles

| Role | Permissions |
|---|---|
| Manager | Manage products, categories, inventory, terminals, users. View all sales and dashboard. |
| Cashier | Log in, search products, create sales, record payments, view own sales. |

## 3. MVP Functional Requirements

| ID | Requirement |
|---|---|
| FR-01 | Users log in with email and password and receive a JWT token. |
| FR-02 | Role-based access: Manager and Cashier see different features. |
| FR-03 | Manager can add, edit, deactivate and search products. |
| FR-04 | Manager can add, edit and delete categories. |
| FR-05 | Manager can view and adjust stock for each product. |
| FR-06 | Manager can add terminals and set them Active or Inactive. |
| FR-07 | Cashier selects a terminal and creates a sale by adding products and quantities. |
| FR-08 | System calculates subtotal, discount and final amount. |
| FR-09 | System records payment as Cash or Card. |
| FR-10 | Stock is reduced centrally when a sale is completed. |
| FR-11 | Users can view sales history. Cashiers see only their own sales. |
| FR-12 | System generates receipt data for a completed sale. |
| FR-13 | Manager dashboard shows today's sales, low stock items and top products. |

## 4. Business Rules

| ID | Rule |
|---|---|
| BR-01 | A sale cannot be completed if any item quantity is more than available stock. |
| BR-02 | Stock reduction and sale creation happen inside one database transaction. |
| BR-03 | Two terminals must never sell the same remaining stock. |
| BR-04 | The product price is copied into the sale item at sale time. Later price changes do not affect old sales. |
| BR-05 | Final amount = Subtotal - Discount. Discount cannot be more than the subtotal. |
| BR-06 | Cash payment must be at least the final amount. Card payment must equal the final amount. |
| BR-07 | A product that has sales is deactivated, never deleted. |
| BR-08 | Only Active terminals can process sales. |
| BR-09 | Passwords are stored only as hashes. |
| BR-10 | Quantity, price and stock values can never be negative. |

## 5. Non-Functional Requirements

- REST API with Swagger documentation.
- All endpoints except login require authentication.
- Input validation on every request.
- Secrets are kept in environment variables, never in GitHub.
- Responsive web interface.

## 6. Out of Scope for MVP

Barcode scanner, receipt printing, refunds, suppliers, purchase orders, audit logs, offline mode.