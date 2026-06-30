# NeciAI — Intelligent Financial Data Analysis Platform

<div align="center">

![NeciAI Dashboard](docs/diagrams/neciai%20wireframe.png)

[![Live Demo](https://img.shields.io/badge/Live%20Demo-neciai--ai.vercel.app-2563eb?style=for-the-badge&logo=vercel)](https://neciai-ai.vercel.app)
[![API Docs](https://img.shields.io/badge/API%20Docs-Swagger%20UI-85ea2d?style=for-the-badge&logo=swagger)](https://neciai-production.up.railway.app/swagger)
[![GitHub](https://img.shields.io/badge/Source-GitHub-181717?style=for-the-badge&logo=github)](https://github.com/MrMustSeeUs/neciai)

![C#](https://img.shields.io/badge/C%23-ASP.NET%20Core%208-512BD4?style=flat-square&logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?style=flat-square&logo=react)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-336791?style=flat-square&logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=flat-square&logo=docker)
![Railway](https://img.shields.io/badge/Railway-Deployed-0B0D0E?style=flat-square&logo=railway)
![Vercel](https://img.shields.io/badge/Vercel-Deployed-000000?style=flat-square&logo=vercel)

</div>

---

## Overview

NeciAI is a full-stack financial data analysis platform that enables business professionals to manage financial records, detect anomalies, generate professional PDF and Excel reports, and search across their financial data in real time. The name derives from the Nahuatl word *Neciz* — "to become clear" — reflecting the application's purpose of bringing clarity to complex financial information.

The platform is built on a clean separation of concerns: a RESTful C# backend API, a responsive React frontend, and a serverless PostgreSQL database — all deployed independently to cloud infrastructure with zero downtime.

---

## Live Demo

| Resource | URL |
|---|---|
| **Application** | https://neciai-ai.vercel.app |
| **API Documentation** | https://neciai-production.up.railway.app/swagger |

**Demo credentials:**
```
Email:    admin@neciai.app
Password: Admin@NeciAI2026!
```

---

## Features

- **JWT Authentication** — Secure login with token-based authentication and role-based access control
- **Financial Records** — Full CRUD operations with category tagging, anomaly detection scoring, and soft deletion for audit compliance
- **Real-time Dashboard** — Revenue vs. Expenses line chart and Monthly Breakdown bar chart powered by Recharts, with live summary metrics
- **PDF & Excel Reports** — Generate professional financial reports with title, timestamp, multi-column data tables, and summary statistics using iText7 and EPPlus
- **Full-text Search** — Keyword search across record titles, descriptions, categories, and tags returning multiple results instantly
- **Swagger UI** — Interactive API documentation available in production for all endpoints
- **Fully Responsive** — Mobile-first design that scales cleanly from mobile to desktop

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Client Browser                       │
└─────────────────────┬───────────────────────────────────┘
                      │ HTTPS
┌─────────────────────▼───────────────────────────────────┐
│              React 18 + Vite + Tailwind CSS              │
│                   Hosted on Vercel CDN                   │
└─────────────────────┬───────────────────────────────────┘
                      │ REST API (HTTPS + JWT)
┌─────────────────────▼───────────────────────────────────┐
│            C# ASP.NET Core 8 REST API                    │
│         Containerized via Docker on Railway              │
│  Controllers → Services → Entity Framework Core → DB    │
└─────────────────────┬───────────────────────────────────┘
                      │ SSL/TLS
┌─────────────────────▼───────────────────────────────────┐
│          PostgreSQL — Neon Serverless Cloud              │
└─────────────────────────────────────────────────────────┘
```

---

## Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| C# ASP.NET Core 8 | REST API framework |
| Entity Framework Core | ORM and database migrations |
| ASP.NET Identity | User management and password hashing |
| JWT Bearer Authentication | Stateless API security |
| iText7 | PDF report generation |
| EPPlus | Excel report generation |
| Npgsql | PostgreSQL driver |

### Frontend
| Technology | Purpose |
|---|---|
| React 18 | UI component framework |
| Vite | Build tool and dev server |
| Tailwind CSS | Utility-first styling |
| Recharts | Data visualization |
| React Router v6 | Client-side routing |
| Axios | HTTP client |

### Infrastructure
| Technology | Purpose |
|---|---|
| Railway | Backend hosting (Docker, always-on) |
| Vercel | Frontend hosting (global CDN) |
| Neon | Serverless PostgreSQL |
| Docker | Backend containerization (multi-stage build) |

### Testing
| Technology | Purpose |
|---|---|
| xUnit | Unit testing framework |
| EF Core InMemory | In-memory database for test isolation |
| Moq | Dependency mocking |
| FluentAssertions | Expressive test assertions |

---

## Object-Oriented Design

The backend demonstrates the three core OOP principles through deliberate design decisions:

### Inheritance
`BaseEntity` is an abstract parent class providing shared audit fields (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) to all model classes. `FinancialRecord`, `Report`, and `Transaction` all inherit from `BaseEntity` without redefining these fields. `ApplicationUser` extends ASP.NET Identity's `IdentityUser` class to add application-specific profile fields.

### Polymorphism
All three model classes override the `GetSummary()` method defined as `virtual` in `BaseEntity`, each returning a different formatted summary string appropriate to its data type. The `OnBeforeSave()` virtual method is also overridden in `FinancialRecord` to normalize data before persistence.

### Encapsulation
`FinancialRecord` uses a private backing field `_amount` for the `Amount` property. The property setter enforces a business rule — any attempt to set a negative amount throws an `ArgumentException` — hiding the validation logic behind a clean public interface.

---

## Project Structure

```
neciai/
├── src/
│   ├── backend/
│   │   └── NeciAI.API/
│   │       ├── Controllers/       # AuthController, FinancialRecordsController, ReportsController
│   │       ├── Data/              # NeciAIDbContext with global query filters
│   │       ├── DTOs/              # Request and response data transfer objects
│   │       ├── Interfaces/        # IFinancialService, IReportService
│   │       ├── Migrations/        # EF Core database migrations
│   │       ├── Models/            # BaseEntity, ApplicationUser, FinancialRecord, Report, Transaction
│   │       ├── Services/          # FinancialService, ReportService
│   │       ├── Dockerfile         # Multi-stage Docker build
│   │       └── Program.cs         # Application configuration and DI registration
│   └── frontend/
│       └── neciai-dashboard/
│           ├── src/
│           │   ├── components/    # Layout with responsive sidebar
│           │   ├── context/       # AuthContext with JWT management
│           │   ├── pages/         # Dashboard, Records, Reports, Search, Login
│           │   └── services/      # Axios API client
│           └── vercel.json        # SPA routing configuration
├── tests/
│   └── backend-tests/
│       └── NeciAI.Tests/
│           └── FinancialRecordTests.cs  # 14 unit tests
└── docs/
    └── diagrams/
        ├── neciai uml.png         # UML class diagram
        └── neciai wireframe.png   # Low-fidelity wireframe
```

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL (or a [Neon](https://neon.tech) free account)

### Backend Setup

```bash
# Navigate to the backend project
cd src/backend/NeciAI.API

# Restore dependencies
dotnet restore

# Set your database connection string in appsettings.Development.json
# "DefaultConnection": "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"

# Apply database migrations
dotnet ef database update

# Start the API (available at http://localhost:5005)
dotnet run
```

### Frontend Setup

```bash
# Navigate to the frontend project
cd src/frontend/neciai-dashboard

# Install dependencies
npm install

# Start the development server (available at http://localhost:5173)
npm run dev
```

### Running Tests

```bash
cd tests/backend-tests/NeciAI.Tests
dotnet test --verbosity normal
```

Expected output:
```
Test summary: total: 14, failed: 0, succeeded: 14, skipped: 0, duration: 4.3s
```

---

## API Reference

The full interactive API documentation is available via Swagger UI at:
```
https://neciai-production.up.railway.app/swagger
```

Key endpoint groups:

| Group | Endpoints |
|---|---|
| **Auth** | `POST /api/Auth/register`, `POST /api/Auth/login`, `GET /api/Auth/profile` |
| **Financial Records** | Full CRUD + search + category filter + date range filter |
| **Reports** | Generate PDF, Generate Excel, Download, Delete |

All endpoints except `register` and `login` require a valid JWT Bearer token.

---

## Deployment

The application is deployed across three cloud platforms:

| Layer | Platform | Details |
|---|---|---|
| Frontend | Vercel | Deployed via Vercel CLI, global CDN, SPA routing via `vercel.json` |
| Backend | Railway | Docker container, multi-stage build, environment variables via Railway dashboard |
| Database | Neon | Serverless PostgreSQL, connection via SSL, EF Core migrations |

### Docker

The backend uses a multi-stage Dockerfile:
- **Build stage** — Microsoft .NET 8 SDK image compiles and publishes the application
- **Final stage** — Microsoft ASP.NET Core 8 runtime image runs the compiled output

This pattern reduces the final image size by excluding build tooling from the production container.

---

## Unit Tests

14 unit tests cover the core business logic:

| Category | Tests |
|---|---|
| Encapsulation | Negative amount rejected, positive amount accepted |
| Polymorphism | `GetSummary()` returns class-specific output in `FinancialRecord` and `Report` |
| Inheritance | Child classes correctly inherit `BaseEntity` fields |
| CRUD | Create, Read (all), Update, Delete (soft) |
| Search | Multiple rows returned for keyword match |
| Security | Unauthorized user receives null, not another user's record |
| Date Range | Records filtered correctly by start and end date |
| OnBeforeSave | Whitespace trimmed before persistence |

---

## Author

**Abraham Macias**
Full Stack Software Engineer — B.S. Software Engineering, Western Governors University

[![GitHub](https://img.shields.io/badge/GitHub-MrMustSeeUs-181717?style=flat-square&logo=github)](https://github.com/MrMustSeeUs)

---

## License

This project is open source and available under the [MIT License](LICENSE).
