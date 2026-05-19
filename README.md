<div align="center">
  <img src="Assets/Image/Logo/El-Sim_Logo_Icon.png" alt="El-Sim logo" width="96" />

  # El-Sim ✈️

  <img src="https://readme-typing-svg.demolab.com?font=Inter&weight=700&size=24&duration=2600&pause=700&color=17A2B8&center=true&vCenter=true&width=760&lines=Digital+eSIM+and+connectivity+platform;Plans%2C+Pass%2C+Global+Beta%2C+Wi-Fi+and+wallet;ASP.NET+Core+MVC%2C+SQL+Server%2C+Stripe+and+clean+UX" alt="Animated typing headline" />

  <p>
    <strong>An eSIM and connectivity web project with responsive public pages, account tools, wallet top-ups, product purchases, and an admin panel.</strong>
  </p>

  <p>
    <img src="https://img.shields.io/badge/Frontend-HTML%20%7C%20CSS%20%7C%20JavaScript-ffb703?style=for-the-badge&logo=javascript&logoColor=111111" alt="Frontend badge" />
    <img src="https://img.shields.io/badge/Backend-ASP.NET%20Core-512bd4?style=for-the-badge&logo=dotnet&logoColor=ffffff" alt="ASP.NET Core badge" />
    <img src="https://img.shields.io/badge/Database-SQL%20Server-cc2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=ffffff" alt="SQL Server badge" />
    <img src="https://img.shields.io/badge/Payments-Stripe-635bff?style=for-the-badge&logo=stripe&logoColor=ffffff" alt="Stripe badge" />
  </p>

  <p>
    <a href="#current-features"><img src="https://img.shields.io/badge/Features-Updated-17a2b8?style=flat-square" alt="Features link" /></a>
    <a href="#aspnet-core-app"><img src="https://img.shields.io/badge/App-ASP.NET%20MVC-512bd4?style=flat-square" alt="ASP.NET MVC link" /></a>
    <a href="#admin-area"><img src="https://img.shields.io/badge/Admin-Panel-222222?style=flat-square" alt="Admin panel link" /></a>
    <a href="#useful-commands"><img src="https://img.shields.io/badge/Run-Commands-2ea44f?style=flat-square" alt="Commands link" /></a>
  </p>
</div>

---

## ✨ Overview

El-Sim is an eSIM and connectivity web project for Azerbaijan and travel users. The repository contains a static HTML/CSS/JavaScript prototype at the root and a full ASP.NET Core MVC application in `.NET/El-Sim.NET`.

## 🚀 Current Features

- 📱 Responsive public pages for Home, Plans, Pass, Global Beta, Wi-Fi, Login, and Profile
- 🎨 Static frontend prototype with shared styling, carousel behavior, mobile navigation, and language switching
- 🧩 ASP.NET Core MVC app with Razor views that mirrors and extends the static pages
- 🔐 Cookie-based authentication with registration, login, logout, and optional two-factor verification
- 👤 User profile management with editable personal data and profile image upload/delete
- 🖼️ Profile image and slider image processing through infrastructure services
- 📦 Product catalog for local plans, Pass packages, Global Beta packages, and optical internet packages
- 💳 Wallet balance, Stripe Checkout top-up flow, and exchange-rate conversion support
- 🧾 Product purchase tracking with receipts and seeded demo purchase data
- 🛠️ Admin dashboard with users, products, product categories, sliders, and purchases
- ⚙️ Admin actions for blocking users, toggling 2FA, editing products, managing sliders, cancelling purchases, and refunding purchases
- 🗄️ SQL Server persistence through Entity Framework Core

## 🧱 Tech Stack

| Layer | Tools |
| --- | --- |
| Static frontend | HTML5, CSS3, vanilla JavaScript |
| Web app | ASP.NET Core MVC, Razor Views, .NET 10 |
| Data | Entity Framework Core, SQL Server LocalDB / SQL Server |
| Payments | Stripe Checkout |
| Auth | ASP.NET Core Cookie Authentication, password hashing, optional 2FA |
| Media | ImageSharp-based image processing |

## 📁 Project Structure

```text
.
|-- Assets/
|   |-- Image/
|   |-- Script/
|   `-- Style/
|-- Admin Panel/
|   `-- Admin Assets/
|-- .NET/
|   `-- El-Sim.NET/
|       |-- El-Sim.Application/
|       |-- El-Sim.Domain/
|       |-- El-Sim.Infrastucture/
|       |-- El-Sim.Persistence/
|       |   |-- Entities/
|       |   `-- ElSimDbContext.cs
|       |-- El-Sim.Web/
|       |   |-- Controllers/
|       |   |-- Models/
|       |   |-- Services/
|       |   |-- Views/
|       |   `-- wwwroot/
|       `-- El-Sim.NET.slnx
|-- index.html
|-- plans.html
|-- global.html
|-- pass.html
|-- wifi.html
|-- login.html
`-- README.md
```

## 🖥️ Static Frontend

Open `index.html` directly in a browser to preview the static version.

Main static pages:

- 🏠 `index.html` - landing page, activation steps, simple packages, support sections
- 📶 `plans.html` - local tariffs, eSIM prices, speeds, no-plan pricing
- 🎟️ `pass.html` - monthly Pass membership packages
- 🌍 `global.html` - Global Beta travel data packages
- 🛜 `wifi.html` - optical internet packages and static IP option
- 🔑 `login.html` - login/register prototype with FIN validation

Shared frontend files:

- `Assets/Style/main-style.css`
- `Assets/Style/main-responsive.css`
- `Assets/Script/main-script.js`
- `Assets/Script/lang.js`
- `Assets/Script/carousel.js`

## ⚡ ASP.NET Core App

The main backend/web application is located at:

```bash
cd .NET/El-Sim.NET
dotnet restore
dotnet run --project El-Sim.Web/El-Sim.Web.csproj
```

Then open the local URL printed by `dotnet run`.

Important routes:

- `/`, `/index.html` - home
- `/plans.html` - local plans
- `/pass.html` - Pass packages
- `/global.html` - Global Beta packages
- `/wifi.html` - optical internet packages
- `/login.html` - login/register
- `/profile.html` - authenticated user profile
- `/wallet/topup/success` - Stripe top-up callback
- `/admin` - admin dashboard
- `/admin/users` - user management
- `/admin/products` - product management
- `/admin/sliders` - slider management
- `/admin/purchases` - purchase management

## 🔧 Configuration

The MVC app expects local configuration for:

- `ConnectionStrings:DefaultConnection`
- `Email` or legacy `EmailSettings`
- `Stripe:PublishableKey`
- `Stripe:SecretKey`
- `Stripe:Currency`

Keep real secrets in local `appsettings.json`, user secrets, or environment variables. Do not commit production credentials.

Example shape:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ElSimDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Email": {
    "From": "example@example.com",
    "Password": "local-password-or-app-token",
    "Host": "smtp.example.com",
    "Port": 587
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "Currency": "usd"
  }
}
```

## 🗃️ Data Model Highlights

Persistence entities currently include:

- `Account`
- `AppUser`
- `Product`
- `ProductPurchase`
- `PaymentReceipt`
- `WalletTransaction`
- `HomeSlider`
- `UserAssets`
- `TwoFactorCode`
- `AppSetting`

Startup services seed or initialize account, product, and purchase data when the web app starts.

## 🛡️ Admin Area

The admin area is implemented in `AdminController` with Razor views under `El-Sim.Web/Views/Admin`.

Current admin screens:

- 📊 Dashboard
- 👥 Users
- 📦 Products
- 🧮 Product category editor
- 🖼️ Sliders
- 🧾 Purchases

Current admin capabilities:

- 🔎 Search users and purchases
- 🚫 Block/unblock users
- 🔐 Enable/disable two-factor verification
- ✏️ Add, update, and delete products
- 🖼️ Add, update, and delete slider images
- ↩️ Cancel or refund purchases

## 🧭 Notes For Development

- Root HTML files are the static prototype.
- `.NET/El-Sim.NET/El-Sim.Web` is the actively developed MVC app.
- Runtime uploads live under `wwwroot/Uploads/` and are ignored by Git.
- Build artifacts, `bin/`, `obj/`, logs, local databases, and local config files are ignored.
- The project currently uses `net10.0`; install a compatible .NET SDK before building.

## 🧪 Useful Commands

```bash
# Build the MVC app
dotnet build .NET/El-Sim.NET/El-Sim.Web/El-Sim.Web.csproj

# Run the MVC app
dotnet run --project .NET/El-Sim.NET/El-Sim.Web/El-Sim.Web.csproj
```

## 🗺️ Roadmap Ideas

- ✅ Add automated tests for auth, profile, wallet, and purchase flows
- 🧬 Add database migrations and documented setup scripts
- 🛡️ Add stronger admin authorization boundaries
- 🔒 Add production-safe secret management documentation
- 🌐 Improve translation file encoding and expand language coverage
- 💳 Connect real checkout/purchase fulfillment for all package types

<div align="center">
  <strong>Built for connected journeys. 🌐</strong>
</div>
