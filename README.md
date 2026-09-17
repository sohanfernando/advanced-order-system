# Advanced Order System

A full-stack order management app. Staff can manage products, browse customers, place orders with discounts and cancel orders. Stock is updated automatically.

| Part | Stack |
|---|---|
| [`backend/`](backend) | ASP.NET Core Web API (.NET 10), Entity Framework Core 10, SQL Server LocalDB |
| [`frontend/`](frontend) | Angular 22 (standalone components, signals), Tailwind CSS 4 |

## Features

- **Products:** list and search active products, and create new ones. SKUs must be unique and are stored in upper case.
- **Customers:** list customers, then jump to their orders or start a new order for them.
- **Orders:**
  - Create an order with several items and a percentage discount. Totals are calculated on the server.
  - List orders with status and customer filters and paging.
  - View an order's details, and cancel an order, which returns its items to stock.
- **Stock safety:** stock is reserved with a single conditional database update, so orders placed at the same time can't oversell a product. A cancelled order returns its stock only once.
- **Consistent errors:** validation errors return `400` and missing records return `404`, both as `{ "message": "..." }`. The frontend shows these messages to the user.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (installed with Visual Studio, or on its own)
- EF Core CLI: `dotnet tool install --global dotnet-ef`
- Node.js `^22.22.3`, `^24.15.0` or `>=26` (required by Angular 22), and npm

## Getting started

### 1. Backend

```bash
cd backend
dotnet restore
dotnet ef database update   # creates AdvancedOrderDb and seeds sample data
dotnet run                  # http://localhost:5264
```

The connection string is in [`backend/appsettings.json`](backend/appsettings.json):

```
Server=(localdb)\MSSQLLocalDB;Database=AdvancedOrderDb;Trusted_Connection=True;TrustServerCertificate=True;
```

The database is seeded with 5 products (Laptop, Mouse, Keyboard, Monitor, Headphones) and 3 customers.

### 2. Frontend

```bash
cd frontend
npm install
npm start                   # http://localhost:4200
```

Open http://localhost:4200. The frontend calls the API at `http://localhost:5264/api`, which is set in [`frontend/src/app/core/config.ts`](frontend/src/app/core/config.ts).

> Run the backend with the default **http** profile. If you use a different port, update `API_BASE_URL` in `config.ts`. If you serve the frontend from a different origin, add it to `Cors:AllowedOrigins` in `appsettings.json`.

## API

Base URL: `http://localhost:5264/api`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/products?search=` | Active products, filtered by name or SKU |
| `GET` | `/products/{id}` | One active product |
| `POST` | `/products` | Create a product |
| `GET` | `/customers` | All customers |
| `GET` | `/customers/{id}` | One customer |
| `GET` | `/orders?status=&customerId=&page=1&pageSize=10` | Paged order list, newest first (`pageSize` max 100) |
| `GET` | `/orders/{id}` | Order with its items |
| `POST` | `/orders` | Create an order |
| `PATCH` | `/orders/{id}/cancel` | Cancel an order and return its stock |

Example requests for every endpoint are in [`backend/backend.http`](backend/backend.http). In Development, the OpenAPI document is at `/openapi/v1.json`.

**Create order request**

```json
{
  "customerId": 1,
  "discountPercent": 10,
  "items": [
    { "productId": 2, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```

### Order rules

- The customer must exist, and the order needs at least one item.
- The discount must be between 0 and 100 (%).
- Each product can appear only once in an order, must be active, and must have enough stock.
- Quantities must be at least 1.
- Line totals, subtotal, discount and total are rounded to 2 decimal places. The price at the time of purchase is stored on each order item.
- Order status is `Confirmed` when the order is created and `Cancelled` after it is cancelled. An order can't be cancelled twice.

## Project structure

```
backend/
├── Controllers/        API endpoints (Order, Product, Customer)
├── Services/           Business rules and validation
├── Repositories/       Data access (EF Core)
├── Data/               ApplicationDbContext, seed data, unit of work
├── Middleware/         Global exception handler (400 / 404 / 500)
├── Models/
│   ├── Entities/       Product, Customer, Order, OrderItem, OrderStatus
│   └── DTOs/           Request and response models
└── Migrations/         EF Core migrations

frontend/src/app/
├── core/               API services, models, config, error helper
├── shared/             Status badge, alert, money and date pipes, validators
└── features/
    ├── orders/         order-list, order-create, order-detail
    ├── products/       product-list (with create form)
    ├── customers/      customer-list
    └── not-found/
```

## Useful commands

| Where | Command | Purpose |
|---|---|---|
| `backend/` | `dotnet build` | Compile the API |
| `backend/` | `dotnet ef migrations add <Name>` | Add a migration after changing the model |
| `frontend/` | `npm run build` | Production build to `dist/` |
| `frontend/` | `npm test` | Run unit tests (Vitest) |
