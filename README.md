<div align="center">
  <img src="Assets/Image/Logo/El-Sim_Logo_Icon.png" alt="El-Sim logo" width="96" />

  # El-Sim ✈️

  <img src="https://readme-typing-svg.demolab.com?font=Inter&weight=700&size=24&duration=2600&pause=700&color=17A2B8&center=true&vCenter=true&width=640&lines=Travel+smarter+with+digital+eSIMs;Fast+plans%2C+global+coverage%2C+clean+UX;Built+with+HTML%2C+CSS%2C+JavaScript+and+.NET" alt="Animated typing headline" />

  <p>
    <strong>A polished eSIM web experience with responsive pages, authentication, profile management, and a .NET backend.</strong>
  </p>

  <p>
    <img src="https://img.shields.io/badge/Frontend-HTML%20%7C%20CSS%20%7C%20JS-ffb703?style=for-the-badge" alt="Frontend badge" />
    <img src="https://img.shields.io/badge/Backend-ASP.NET%20Core-512bd4?style=for-the-badge" alt="ASP.NET Core badge" />
    <img src="https://img.shields.io/badge/Database-SQL%20Server-cc2927?style=for-the-badge" alt="SQL Server badge" />
  </p>
</div>

---

## ✨ Overview

El-Sim is a modern eSIM website designed for travelers who want quick access to mobile data plans, global coverage information, Wi-Fi/pass options, and account tools. The repository contains a static frontend prototype at the root and an ASP.NET Core MVC application inside the `.NET/El-Sim.NET` workspace.

## 🚀 Features

- 🌍 Multi-page eSIM storefront: home, plans, global, pass, Wi-Fi, and login pages
- 📱 Responsive layout with dedicated mobile styling
- 🎠 Carousel and interactive UI behavior with vanilla JavaScript
- 🔐 Cookie-based authentication in the .NET MVC app
- 👤 Profile page with editable user details
- 🖼️ Profile image upload and WebP processing
- ✉️ Email-based two-factor verification support
- 🗄️ Entity Framework Core persistence with SQL Server

## 🧱 Tech Stack

| Layer | Tools |
| --- | --- |
| Frontend | HTML5, CSS3, JavaScript |
| Backend | ASP.NET Core MVC, .NET |
| Data | Entity Framework Core, SQL Server |
| Auth | Cookie Authentication, password hashing, optional 2FA |
| Assets | Custom images, sliders, logo, favicon |

## 📁 Project Structure

```text
.
├── Assets/
│   ├── Image/
│   ├── Script/
│   └── Style/
├── .NET/
│   └── El-Sim.NET/
│       ├── El-Sim.Application/
│       ├── El-Sim.Domain/
│       ├── El-Sim.Infrastucture/
│       ├── El-Sim.Persistence/
│       └── El-Sim.Web/
├── index.html
├── plans.html
├── global.html
├── pass.html
├── wifi.html
└── login.html
```

## ⚡ Getting Started

### Static frontend

Open `index.html` in a browser to preview the static version.

### ASP.NET Core app

```bash
cd .NET/El-Sim.NET
dotnet restore
dotnet run --project El-Sim.Web/El-Sim.Web.csproj
```

Then open the local URL printed by `dotnet run`.

## 🛡️ Git Hygiene

This project ignores local IDE files, build outputs, uploaded user files, and secret configuration files. Keep generated folders like `bin/`, `obj/`, `.vs/`, and `wwwroot/Uploads/` out of commits.

## 🧭 Roadmap Ideas

- 🧪 Add automated tests for authentication and profile flows
- 🧭 Add plan filtering by country or region
- 💳 Connect checkout/payment flow
- 🌐 Expand language support
- 📊 Add admin analytics for users and plans

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run the app locally
5. Open a pull request

<div align="center">
  <strong>Built for connected journeys. 🌐</strong>
</div>
