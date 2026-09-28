# Smart Inventory & Order Management System

## Project Overview

Smart Inventory & Order Management System (SmartIOMS) is a backend Web API application developed using C#, ASP.NET Core Web API, Entity Framework Core, SQL Server and JWT Authentication.

The system allows customers to browse products, manage their shopping cart and place orders. Admin users can manage products and monitor inventory.

The application also includes stock validation, order cancellation, low-stock reporting and concurrency-safe stock updates.

---

## Technologies Used

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- LINQ
- JWT Authentication
- BCrypt Password Hashing
- Swagger / OpenAPI
- REST APIs

---

## Features

### Customer Features

- User registration
- JWT-based login
- Browse products
- Search products
- Product pagination
- Add products to cart
- Update cart quantity
- Remove cart items
- Clear cart
- Place orders
- View own order history
- View individual own orders
- Order cancellation
- Automatic stock restoration after cancellation

### Admin Features

- Admin login
- Create products
- Update products
- Delete products
- View products
- Low-stock report
- Role-based authorization

### Security

- JWT authentication
- Role-based authorization
- Customer-specific cart access
- Customer-specific order access
- Password hashing using BCrypt
- Global exception handling
- Input validation

### Inventory Management

- Stock validation before adding products to cart
- Stock validation during order placement
- Inventory cannot become negative
- Inventory transaction tracking
- Optimistic concurrency using EF Core RowVersion
- Automatic stock restoration when an order is cancelled

---

## Project Structure

```text
SmartIOMS
│
├── Controllers
├── Data
├── DTOs
├── Middleware
├── Migrations
├── Models
├── Services
├── Program.cs
├── appsettings.json
├── SmartIOMS.csproj
└── README.md