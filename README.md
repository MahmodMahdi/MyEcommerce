# 🛒 MyEcommerce - Enterprise N-Tier ASP.NET Core Solution

MyEcommerce is a high-performance, secure, and scalable e-commerce platform built using **ASP.NET Core MVC**. This project is not just a simple CRUD application; it is a feature-rich system designed to handle the complexities of real-world online trading, focusing on **Transactional Integrity**, **Stock Security**, and **Scalability**.

---

## 🏗 System Architecture & Design Patterns

The application is built following an **N-Tier Architecture** to ensure separation of concerns and maintainability:

* **Domain Layer:** Contains POCO models, interfaces, and business entities.
* **DataAccess Layer:** Implements Entity Framework Core with the **Generic Repository Pattern** to abstract data logic.
* **Presentation Layer:** ASP.NET Core MVC (Area-based) providing a clean UI for both Customers and Admins.
* **Design Patterns:** Extensive use of **Unit of Work** for atomic transactions and **Dependency Injection** for decoupled service management.

---

## 🚀 Key Technical Challenges & Solutions

### 🛡 Intelligent Inventory Safeguard (Stock Guard)
In high-traffic scenarios, stock levels change rapidly. I implemented a **Multi-Stage Validation Pipeline** to prevent "Overselling":
1.  **UI Level:** Real-time checking to disable "Add to Cart" and display "Out of Stock" labels.
2.  **Summary Cleanup:** A specialized service runs before the user reaches Stripe, automatically scrubbing the cart and adjusting quantities if stock was sold to another customer in the interim.
3.  **Atomic Post-Payment:** Stock is only physically deducted when a verified `Paid` status is returned from the Stripe API.

### 💳 Professional Order Workflow
The order lifecycle mimics enterprise ERP systems:
* **State Machine:** Orders transition through logical states: `Pending` -> `Approved` -> `Processing` -> `Shipped` -> `Cancelled`.
* **Financial Integrity:** Every order is linked to a Stripe `PaymentIntentId`, ensuring a 1:1 match between financial transactions and database records.

---

## 💎 Core Features

### 👤 Customer Experience
* **Smart Shopping Cart:** Hybrid approach where cart counts are persisted in the database and reflected via optimized Session State.
* **Secure Checkout:** Fully integrated with **Stripe Checkout Sessions** for PCI-compliant payment processing.
* **Real-time Feedback:** Integrated **Toastr** and **SweetAlert2** for professional notifications.

### 👮 Admin Control Center
* **Product Management:** Full CRUD with image upload functionality and category hierarchy.
* **Order Management:** A specialized "Control Tower" dashboard to track and manage order lifecycles and revenue.
* **Inventory Alerts:** Visual indicators for low-stock items to prompt restock actions.

---

## 🛠 Tech Stack

| Category | Technology |
| :--- | :--- |
| **Backend** | ASP.NET Core 8.0 (MVC) |
| **ORM / DB** | EF Core & SQL Server |
| **Security** | ASP.NET Identity (RBAC) |
| **Payments** | Stripe API SDK |
| **UI** | Bootstrap 5, JavaScript, Session State |

---

## 📖 Deep Dive: How it Works (Under the Hood)

### 1. Repository & Unit of Work
Ensures the data access logic is centralized. For example, adding to a cart:
```csharp
_UnitOfWork.ShoppingCartRepository.AddAsync(cart);
await _UnitOfWork.CompleteAsync(); // Atomic commit
