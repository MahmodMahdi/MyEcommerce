# MyEcommerce - Enterprise ASP.NET Core MVC Solution

A robust, full-stack e-commerce platform built with **ASP.NET Core MVC**, focusing on complex business logic, real-time inventory synchronization, and secure transaction handling.



## 🚀 The Challenge & The Solution

In most junior projects, inventory is just a simple number. In **MyEcommerce**, I tackled real-world race conditions and data integrity:

* **The Problem:** What if two users add the last item to their carts simultaneously?
* **The Solution:** Implemented a **Multi-Stage Validation Pipeline**:
    * **Pre-checkout sync:** The system automatically scrubs the cart during the summary stage to ensure stock still exists.
    * **Atomic Transactions:** Stock is only deducted upon a verified `paid` status from the Stripe API.
    * **Session Integrity:** Real-time synchronization between the database cart count and the UI session badge.

## 🛠 Tech Stack & Architecture

* **Framework:** ASP.NET Core 8.0 (MVC)
* **Architecture:** N-Tier Architecture with **Repository Pattern** and **Unit of Work**.
* **ORM:** Entity Framework Core with LINQ.
* **Security:** Identity Framework with Role-Based Access Control (Admin/Customer).
* **Payments:** Stripe API Integration (Checkout Sessions).
* **Frontend:** Bootstrap 5, JavaScript, SweetAlert2, and Toastr.

## 💎 Features Highlight

### 🛡 Inventory & Cart Security
* **Smart "Out of Stock" UI:** Buttons automatically disable and change state based on real-time availability.
* **Cart Auto-Adjustment:** If another user buys an item you have in your cart, your cart quantity is automatically capped or removed with a notification.

### 💳 Order Management
* **Stripe Integration:** Handles secure payments and returns metadata for order tracking.
* **Order Tracking:** Admins can manage order statuses (Pending, Approved, Shipped, Cancelled).



## 📖 How it Works (Under the hood)

### 1. Repository Pattern & Unit of Work
Ensures the data access logic is centralized, reducing code duplication and making the application easier to test.
```csharp
// Example of clean Unit of Work usage in the Controller
_UnitOfWork.ShoppingCartRepository.AddAsync(cart);
await _UnitOfWork.CompleteAsync();
