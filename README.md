# 🛒 MyEcommerce - Scalable N-Tier E-Commerce Solution

This project is a high-performance, secure, and scalable e-commerce platform built using **ASP.NET Core MVC**. It demonstrates professional software engineering practices, including **Clean Architecture**, **Generic Repository Pattern**, and **Unit of Work**.



---

## 🏗 System Architecture & Design Patterns

The application is built following an **N-Tier Architecture** to ensure separation of concerns and maintainability:

1.  **Domain Layer:** Contains POCO models, interfaces, and business entities.
2.  **DataAccess Layer:** Implements Entity Framework Core with the **Generic Repository Pattern** to abstract data logic.
3.  **Presentation Layer:** ASP.NET Core MVC (Area-based) providing a clean UI for both Customers and Admins.
4.  **Utilities Layer:** Centralized location for constants, helper classes (e.g., Stripe settings, Role names).

### Key Design Patterns:
* **Unit of Work:** Coordinates the writing of multiple repositories to the database in a single transaction.
* **Dependency Injection:** Extensively used for decoupling services and improving testability.
* **View Models:** Used to pass precisely structured data between Controllers and Views.

---

## 🛠 Advanced Features & Business Logic

### 🔐 Multi-Layer Inventory Protection
One of the most complex parts of this system is the **Stock Management Engine**. It prevents "Overselling" through:
* **Server-Side Validation:** Double-checking stock availability at the moment of adding to cart and again during the checkout summary.
* **Atomic Transactions:** Stock is only deducted when Stripe confirms a `Paid` status.
* **Stale Data Cleanup:** Automatic removal or adjustment of cart items if stock levels change while the user is browsing.

### 💳 Financial Integration (Stripe)
* Integrated **Stripe Checkout Sessions** for PCI-compliant payment processing.
* Metadata tracking to link Stripe sessions with internal Order Headers.
* Automated post-payment workflow (Status update -> Stock deduction -> Email/UI Confirmation).

### 👮 Admin Control Center
* **Inventory Management:** Full CRUD for Products/Categories with image upload functionality.
* **Order Management:** A specialized dashboard to track order lifecycles (Pending -> Approved -> Shipped -> Cancelled).
* **Stock Alerts:** Visual indicators for low-stock or out-of-stock items.



---

## 🚀 Tech Stack

| Technology | Purpose |
| :--- | :--- |
| **ASP.NET Core 8.0** | Core Backend Framework |
| **EF Core & SQL Server** | Data Persistence & ORM |
| **ASP.NET Identity** | Authentication & Role-Based Authorization |
| **Stripe SDK** | Secure Payment Gateway |
| **Bootstrap 5 & JS** | Responsive UI & Client-side logic |
| **Session State** | Real-time Shopping Cart badge tracking |

---

## 📖 Deep Dive: Order Workflow

1.  **ShoppingCart:** User adds items; system checks stock and creates/updates records.
2.  **Summary:** Final validation of stock vs. requested quantity.
3.  **Stripe Redirect:** User enters payment info on Stripe's secure servers.
4.  **Webhook/Return:** Upon success, `OrderConfirmation` is triggered.
5.  **Database Update:** The system clears the user's cart, updates `OrderHeader`, and reduces `StockQuantity` in the `Products` table.

