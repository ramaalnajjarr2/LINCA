<p align="center">
  <img src="Assets/logo.png" alt="LINCA Logo" width="180">
</p>

<h1 align="center">
LINCA (Link Campus)
</h1>

<p align="center">
A secure web-based marketplace connecting university students across Jordan,
allowing them to showcase, sell, and purchase student-made products in a trusted environment.
</p>

<p align="center">

🎓 Student Marketplace • 🛒 E-Commerce • 🔐 Secure Authentication • 🏪 Multi-Role Platform

</p>

---

# 📖 Overview

LINCA (Link Campus) is a web-based marketplace developed to empower university students in Jordan by providing a dedicated platform where they can showcase, promote, and sell their products and services.

The platform creates a trusted environment by verifying student sellers through their university email addresses and requiring administrative approval before activating seller accounts.

LINCA supports communication between buyers and sellers, secure product management, organized stores, order processing, and multiple user roles while maintaining transparency, quality, and ease of use.

The project was developed using **ASP.NET Core MVC**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Identity**, following modern software engineering principles.

---

# ✨ Key Features

- User Registration & Login
- Secure Authentication using ASP.NET Identity
- University Email Verification
- Multi-Role System
- Seller Upgrade Requests
- Admin Approval Workflow
- Student Stores
- Product Management
- Shopping Cart
- Order Management
- Store Management
- Product Categories
- University-Based Marketplace
- Public Buyer Support
- Responsive User Interface
- Administrative Control Panel

---

# 👥 User Roles

## 👤 Guest

A guest can:

- Browse the home page.
- View available universities.
- Explore student stores.
- Browse available products.
- Register a new account.
- Login securely.

---

## 👥 Customer

After signing in, customers can:

- Browse stores from different universities.
- Search products.
- View detailed product information.
- Add products to the shopping cart.
- Place orders.
- Track their orders.
- Update account information.
- Submit a request to become a seller.

---

## 🛍 Seller

Once the seller request is approved by the administrator, the user becomes a Seller and can:

- Create and manage a personal store.
- Add new products.
- Edit existing products.
- Delete products.
- Receive customer orders.
- Update order status.
- Manage store information.
- Offer products and services.
- Communicate with customers through the platform.

---

## 🛠 Admin

The administrator has full control over the platform and can:

- Review seller requests.
- Approve or reject seller applications.
- Monitor stores.
- Manage products.
- Manage users.
- Monitor platform activity.
- Maintain marketplace quality.
- Ensure platform integrity and security.

---
# 📸 Screenshots

## Shared Pages

### Welcome Page

Displays the landing page where users can explore the platform and access authentication features.

![Welcome Page](Screenshots/Shared/welcome.png)
---

### Login

Secure authentication page for registered users.

![login](Screenshots/Shared/login.png)

---

### Register

Allows new students to create an account using their university email.

![Signin](Screenshots/Shared/signin.png)


---

---

# 👤 Customer Screenshots


---

### Product Details

<p align="center">
  <img src="Screenshots/Customer/details-product.png" width="900">
</p>

---

### Shopping Cart / Orders

<p align="center">
  <img src="Screenshots/Customer/cart.png" width="900">
</p>
---
### Customer Dashboard

<p align="center">
  <img src="Screenshots/Customer/my-orders.png" width="900">
</p>


---

# 🛍 Seller Screenshots

### Seller Dashboard

<p align="center">
  <img src="Screenshots/Seller/orders-state.png" width="900">
</p>

---

### Add Product

<p align="center">
  <img src="Screenshots/Seller/add-product.png" width="900">
</p>

---

### Product Management

<p align="center">
  <img src="Screenshots/Seller/details.png" width="900">
</p>


---

# 🛠 Admin Screenshots

### Admin Dashboard

<p align="center">
  <img src="Screenshots/Admin/Dashboard.png" width="900">
</p>

---

### Seller Requests Management

<p align="center">
  <img src="Screenshots/Admin/seller-requests.png" width="900">
</p>

---

### User Management

<p align="center">
  <img src="Screenshots/Admin/admin-panel.png" width="900">
</p>

# 🔄 System Workflow

1. A student creates an account.
2. The student browses the marketplace as a customer.
3. The student submits a Seller Request.
4. The administrator reviews the request.
5. If approved, the user's role changes to Seller.
6. The seller creates a personal store.
7. Products are added to the store.
8. Customers browse products.
9. Customers place orders.
10. Sellers manage incoming orders.
11. Administrators monitor and manage the entire platform.

---
---

# 🛠 Technologies Used

The project was developed using the following technologies:

### Backend

- ASP.NET Core MVC
- C#
- Entity Framework Core
- ASP.NET Identity

### Database

- SQL Server

### Frontend

- HTML5
- CSS3
- Bootstrap
- JavaScript

### Development Tools

- Visual Studio 2026
- SQL Server Management Studio (SSMS)
- Git
- GitHub

---

# 📂 Project Structure

```text
LINCA
│
├── Controllers
├── Models
├── Views
├── ViewModels
├── Services
├── Validations
├── Migrations
├── wwwroot
├── Properties
├── appsettings.json
├── Program.cs
└── LINCA.sln
```

The project follows the **Model–View–Controller (MVC)** architecture to ensure clean code organization, maintainability, and scalability.

---
# 🚀 How to Run the Project

## Prerequisites

Before running the project, make sure you have the following installed:

- Visual Studio 2022 (ASP.NET and Web Development workload)
- .NET 8 SDK
- SQL Server LocalDB or SQL Server Express
- SQL Server Management Studio (SSMS) *(required only if using the provided database script)*
- Git

---

## 1. Clone the Repository

Open **Command Prompt (CMD)** or **Terminal** and run:

```bash
cd Desktop

git clone https://github.com/ramaalnajjarr2/LINCA.git

cd LINCA
```

---

## 2. Open the Project

Open the solution file:

```text
LINCA.sln
```

using **Visual Studio 2022**.

Visual Studio will automatically restore the required NuGet packages.

If the packages are not restored automatically, right-click the solution and select:

```text
Restore NuGet Packages
```

---

## 3. Build the Project

From the Visual Studio menu, click:

```text
Build → Rebuild Solution
```

Wait until the build completes successfully before continuing.

---

# Option 1 – Create a New Database Using Entity Framework Migrations (Recommended)

This option creates a **new empty SQL Server database** using the Entity Framework Core migrations included in the project.

It is the recommended approach for developers who want to start with a fresh database.

> **Note:** This option creates only the database structure (tables, relationships, constraints, etc.). Any sample data will not be available unless it is seeded by the application.

### Steps

Open:

```text
Tools
    → NuGet Package Manager
        → Package Manager Console
```

Run:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update
```

Entity Framework Core will automatically:

- Create the SQL Server database.
- Create all tables.
- Apply all migrations.
- Configure relationships and constraints.

Once the command finishes successfully, simply run the project.

---

# Option 2 – Use the Preconfigured Database (Recommended for Quick Testing)

The repository includes a complete SQL Server database script with the project schema and sample data.

This option is recommended if you want to explore the system immediately without manually creating test data.

### Steps

Open **SQL Server Management Studio (SSMS)**.

Connect to:

```text
(localdb)\MSSQLLocalDB
```

or your SQL Server instance.

---

Right-click:

```text
Databases
```

Choose:

```text
New Database...
```

Create a database named exactly:

```text
LincaDB9
```

> **Important:** The database name must be **LincaDB9** because the default connection string in `appsettings.json` points to this database.

After the database has been created:

Right-click:

```text
LincaDB9
```

Choose:

```text
New Query
```

Open the following file from the repository:

```text
Database/LincaDB9.sql
```

Copy the entire script into the query window (or open the file directly in SSMS).

Execute the script by clicking:

```text
Execute
```

or simply press:

```text
F5
```

Wait until the execution completes successfully.

Refresh the **LincaDB9** database.

You should now see all project tables together with the included sample data.

---

## 4. Verify the Connection String

Open:

```text
appsettings.json
```

Make sure the connection string matches your database.

Example:

```json
"ConnectionStrings": {
  "LincaPortal": "Server=(localdb)\\MSSQLLocalDB;Database=LincaDB9;Trusted_Connection=True;TrustServerCertificate=True"
}
```

If you created the database using a different name or SQL Server instance, update the connection string accordingly.

---

## 5. Run the Application

Press:

```text
F5
```

or

```text
Ctrl + F5
```

or click:

```text
Start Debugging
```

The application will launch automatically in your default web browser.

Enjoy exploring LINCA! 🎉