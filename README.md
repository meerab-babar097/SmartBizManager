# SmartBiz Manager

A small-business management web app built with ASP.NET Core MVC — inventory, customers, sales, expenses, and reporting for businesses currently running on notebooks and WhatsApp.

## Features

- **Dashboard** — live stock alerts, today's sales, monthly expenses, inventory value
- **Products** — CRUD with SKU uniqueness, low-stock thresholds, search & filtering
- **Customers** — purchase history and running outstanding balance per customer
- **Sales** — multi-item sale entry with server-side stock validation inside a database transaction, auto-generated invoice numbers, partial payments
- **Expenses** — categorized tracking with date-range filtering
- **Reports** — revenue, cost of goods sold, expenses, and estimated profit for any date range, plus top-selling products
- **Printable invoices** — clean, standalone print/PDF view per sale

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C#, ASP.NET Core MVC (.NET 9) |
| ORM | Entity Framework Core 9, Code-First Migrations |
| Database | SQL Server (LocalDB in development) |
| Frontend | Bootstrap 5, vanilla JavaScript |

## Architecture

Standard MVC with one deliberate exception: `Sale` creation goes through a dedicated `SaleService` rather than living in the controller, because it needs to validate stock across multiple products and commit everything inside a single database transaction — if any line item fails, nothing is saved, including inventory changes already made in memory. Every other module (Products, Customers, Expenses) is plain controller-to-`DbContext` CRUD, since there's no equivalent multi-step business rule to protect.

## Database design

```
Category ──1───∞── Product ──1───∞── SaleItem ──∞───1── Sale ──∞───1── Customer
Expense (standalone)
```

`SaleItem` snapshots each product's name, selling price, and purchase cost at the moment of sale — so editing a product later never rewrites historical invoices, and profit reports stay accurate even after prices change.

## Running locally

1. Clone the repo
2. Ensure you have the .NET 9 SDK and SQL Server LocalDB installed
3. `dotnet restore`
4. `dotnet ef database update` (or just `dotnet run` — migrations apply automatically on startup)
5. Open the URL shown in the console

The database seeds itself with sample products and customers on first run.

## Future improvements

- ASP.NET Core Identity for authentication and multi-business data isolation
- Migrate target framework from `net9.0` to `net10.0` (LTS) once tooling supports it
- Python-based sales trend/demand-forecasting service, called over REST from the MVC app

## Author

Meerab Babar — BS Economics with Data Science, COMSATS University Islamabad, Lahore Campus
