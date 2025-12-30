 🛍️ ShopSphere E-Commerce Platform

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Build](https://img.shields.io/badge/Build-Passing-brightgreen)
![Coverage](https://img.shields.io/badge/Coverage-95%25-yellow)

Enterprise-Grade E-Commerce Web Application built with ASP.NET MVC 8.0, following Clean Architecture, SOLID principles, and modern .NET development practices.  

The platform includes a robust admin dashboard, customer-facing web pages, secure authentication, payment integration, email notifications, advanced logging & monitoring, and enhanced user experience.

---

 📋 Table of Contents

- [✨ Features](#-features)  
- [🏗️ Architecture](#️-architecture)  
- [🚀 Quick Start](#-quick-start)  
- [🔐 Authentication & Security](#-authentication--security)  
- [🛠️ Tech Stack](#️-tech-stack)  
- [📊 Design Patterns](#-design-patterns)  
- [🤝 Contributing](#-contributing)  

---

 ✨ Features

 🔐 Advanced Authentication & Security
- Cookie-based sessions for admin and customer panels  
- Google OAuth 2.0 login for admin with One-Tap sign-in  
- Role-based authorization (Admin, Editor, Customer)  
- ASP.NET Core Identity integration with scaffolded pages  
- Account lockout:  
  1. Admin manually locks user  
  2. Automatic lockout after 5 failed login attempts  
- Email confirmation for registration and password reset  
- Email notifications for order confirmation and shipped orders  

 📦 E-Commerce Core
- Product catalog with categories & brands  
- Shopping Cart with Session + Redis caching  
- Order management with email notifications  
- Stripe payment integration  
- Delivery methods management  
- Product reviews and ratings  
- Image service for product images  

 🎨 Admin Dashboard
- Modern MVC interface with responsive design  
- CRUD operations for Products, Categories, Users, Orders  
- Real-time statistics & analytics on dashboard  
- Integration with TinyMCE, DataTables, SweetAlert, Toaster notifications  
- Admin-only role-based access  

 🛒 Customer Layer
- Product browsing (Home / Index, Product Details)  
- Add products to Cart, view Cart Summary, checkout  
- Session-based cart updates with real-time changes  
- Paginated product listings  
- Notifications and enhanced UX for orders and cart updates  

 ⚡ Technical Excellence
- Clean Architecture implementation (Presentation → App Layer → Domain → DAL)  
- Application Layer Extensions for service registration  
- Service Interfaces, ViewModels, Mapping Profiles  
- Generic Repository & Unit of Work  
- Specification Pattern for queries  
- Data annotations for validations  
- Serilog logging & monitoring for all errors  
- Async/await for non-blocking I/O  

---

 🏗️ Architecture

Presentation Layer
├── Areas
│ ├── Customer
│ │ ├── Cart (Index, Summary, Order Confirmation)
│ │ ├── Home (Index, Product Details, AddToCart)
│ └── Admin
│ ├── Category (Create/Edit/Index/Delete)
│ ├── Product (Create/Edit/Delete)
│ ├── Order (Details/Index)
│ ├── User (Index, Lock/Unlock)
│ └── Dashboard (Statistics, Charts)

Application Layer
├── Extensions (Service Registration)
├── Interfaces (Service Abstractions)
├── Services (Business Logic)
└── ViewModels + AutoMapper Profiles

Domain Layer
├── Models (Entities)
└── Interfaces (Repository Contracts)

DAL (Data Access Layer)
├── Data (DbContext)
├── DataSeeding
├── Repositories (Generic + Specific)
└── Migrations

markdown
Copy code

Utilities
- EmailSettings, StripeInfo, Helpers for static content   

---
🔄 Order Processing Flow
The platform implements a robust two-phase order processing system:
Show Image <img width="1536" height="1024" alt="d520c872-9e10-486a-a825-93e9676ab99c" src="https://github.com/user-attachments/assets/0512f69b-cc79-4288-8afe-d3ccca2b4153" />
Checkout Process

User Clicks "Checkout"

System calls CreateOrderAsync() to initialize order


Pending Order Check

Yes: Reuses existing session and returns old Stripe URL
No: Creates new order with order details


Stripe Session Creation

Creates or updates Stripe checkout session
Saves and commits order to database
Redirects user to Stripe checkout page



Order Confirmation

User Returns from Stripe

System calls OrderConfirmation(id) to verify payment


Payment Verification

Checks if order has been paid via Stripe API


Transaction Processing

Begin Transaction: Ensures atomic operations
Stock Availability Check:

Yes: Updates stock levels and approves order
No: Marks order as "Approved Stock Issue"


Clear User Cart: Removes items from shopping cart
Commit & Save: Persists all changes to database



Color Coding Legend

🟩 Green: VM & Order Handling operations
🟦 Blue: Stripe integration checks
🟧 Orange: Transaction and database operations
🟥 Red: Stock handling and error states

 🚀 Quick Start

 Prerequisites
- .NET MVC 8.0 SDK  
- SQL Server (LocalDB/Express/Full)
- Smtp Email Integration  
- Stripe Account  
- Google Cloud Console (OAuth)  

Installation
```bash
git clone <your-repo-url>
cd MyEcommerce
dotnet restore
Configure Secrets
json
Copy code
{
  "AdminSettings": {
    "Email": "admin@shopsphere.com",
    "Password": "StrongPassword!"
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  },
  "EmailSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "FromEmail": "noreply@shopsphere.com",
    "Password": "EmailPassword"
  },
  "Authentication": {
    "Google": {
      "ClientId": "...",
      "ClientSecret": "..."
    }
  }
}
Database
bash
Copy code
cd MyEcommerce.PresentationLayer
dotnet ef database update
Run Application
bash
Copy code
dotnet run
Access Admin Dashboard

Access Customer pages

🔐 Authentication & Security
Cookie-based authentication for Admin & Customer

Role-based authorization: [Authorize(Roles="Admin")]

Account lockout & password hashing

Google OAuth 2.0 for Admin Dashboard

Email confirmations & password reset workflow

🛠️ Tech Stack
Backend: ASP.NET Core 8.0, EF Core

Authentication: ASP.NET Core Identity, Google OAuth

Database & Caching: SQL Server, Redis

Payment: Stripe API

Frontend/Admin: MVC + Razor Pages, DataTables, TinyMCE, SweetAlert

Logging: Serilog

📊 Design Patterns
Repository	Abstract data access	DAL/Repositories
Unit of Work	Transaction management	DAL/Repositories
Dependency Injection	Loose coupling	Throughout

🤝 Contributing
PRs welcome!

Follow Clean Architecture principles

Write unit tests for services & repositories

Keep sensitive info in User Secrets or Environment Variables

✅ Production-ready, secure, and scalable e-commerce web application with rich Admin & Customer UX, advanced security, payments, and notifications.
