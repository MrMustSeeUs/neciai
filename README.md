# NeciAI — Intelligent Financial Data Analysis Platform

[![Live Demo](https://img.shields.io/badge/Live%20Demo-neciai--ai.vercel.app-2563eb?style=for-the-badge&logo=vercel)](https://neciai-ai.vercel.app)
[![API Docs](https://img.shields.io/badge/API%20Docs-Swagger%20UI-85ea2d?style=for-the-badge&logo=swagger)](https://neciai-production.up.railway.app/swagger)
[![GitHub](https://img.shields.io/badge/Source-GitHub-181717?style=for-the-badge&logo=github)](https://github.com/MrMustSeeUs/neciai)

[![C#](https://img.shields.io/badge/C%23-ASP.NET%20Core%208-512BD4?style=flat&logo=dotnet)](https://github.com/MrMustSeeUs/neciai)
[![React](https://img.shields.io/badge/React-18-61DAFB?style=flat&logo=react)](https://github.com/MrMustSeeUs/neciai)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-336791?style=flat&logo=postgresql)](https://github.com/MrMustSeeUs/neciai)
[![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=flat&logo=docker)](https://github.com/MrMustSeeUs/neciai)
[![Railway](https://img.shields.io/badge/Railway-Deployed-0B0D0E?style=flat&logo=railway)](https://github.com/MrMustSeeUs/neciai)
[![Vercel](https://img.shields.io/badge/Vercel-Deployed-000000?style=flat&logo=vercel)](https://github.com/MrMustSeeUs/neciai)

![NeciAI Dashboard](docs/diagrams/neciai%20wireframe.png)

---

## Overview

Financial teams lose time reconciling scattered spreadsheets, spotting anomalies by eye, and manually assembling reports for stakeholders. **NeciAI** solves that: a full-stack platform where business professionals manage financial records, catch anomalies automatically, generate polished PDF and Excel reports, and search across their entire financial history in real time — all from one dashboard.

The name comes from the Nahuatl word *Neciz* — "to become clear" — which is exactly the point: turning scattered financial data into something a stakeholder can actually read and act on.

This isn't a tutorial-follow-along project. It's a three-tier system — a RESTful C# API, a responsive React frontend, and a serverless PostgreSQL database — designed, built, and independently deployed to production by one engineer, end to end.

---

## Live Demo

| Resource | URL |
|---|---|
| **Application** | https://neciai-ai.vercel.app |
| **API Documentation** | https://neciai-production.up.railway.app/swagger |

**Try it yourself** — register a free account directly against the live API and explore the full application with your own data:

```
POST https://neciai-production.up.railway.app/api/Auth/register
```

Fill in the required fields via the Swagger UI linked above (or the "Try it yourself" panel on the login page), then sign in immediately with the account you created. No shared credentials, no waiting for access — the whole thing is self-serve.

---

## Features

- **JWT Authentication** — Token-based login with ASP.NET Identity, password hashing, and account lockout after repeated failed attempts
- **Financial Records Management** — Full CRUD with category tagging, anomaly-detection scoring, and soft deletion for audit-trail compliance
- **Real-Time Dashboard** — Revenue vs. expenses trend line and monthly breakdown chart powered by Recharts, with live summary metrics
- **PDF & Excel Report Generation** — One-click, professional financial reports with titles, timestamps, multi-column tables, and summary statistics, via iText7 and EPPlus
- **Full-Text Search** — Instant keyword search across record titles, descriptions, categories, and tags
- **Interactive API Documentation** — Every endpoint is explorable and testable live via Swagger UI
- **Fully Responsive** — Mobile-first layout that scales cleanly from phone to desktop

---

## Security & Engineering Practices

Beyond the feature set, this project follows practices intended to reflect how production systems are actually secured — not just how they're demoed:

- **Zero hardcoded secrets** — the database connection string and JWT signing key are read exclusively from environment variables at runtime (Railway), never committed to source control
- **Safe credential rotation** — the database seeding logic can rotate the admin password on redeploy without ever storing the plaintext value in code
- **Password hashing via ASP.NET Identity** — no password is ever stored or compared in plaintext
- **Account lockout protection** — five consecutive failed login attempts trigger a 15-minute lockout, mitigating brute-force attempts
- **Self-service demo access** — rather than publishing a standing admin credential publicly, the app exposes a registration endpoint so anyone can create their own scoped account to evaluate the system

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Client Browser                       │
└─────────────────────┬───────────────────────────────────┘
                       │ HTTPS
┌─────────────────────▼───────────────────────────────────┐
│              React 18 + Vite + Tailwind CSS               │
│                   Hosted on Vercel CDN                    │
└─────────────────────┬───────────────────────────────────┘
                       │ REST API (HTTPS + JWT)
┌─────────────────────▼───────────────────────────────────┐
│            C# ASP.NET Core 8 REST API                     │
│         Containerized via Docker on Railway                │
│  Controllers → Services → Entity Framework Core → DB      │
└─────────────────────┬───────────────────────────────────┘
                       │ SSL/TLS
┌─────────────────────▼───────────────────────────────────┐
│          PostgreSQL — Neon Serverless Cloud                │
└─────────────────────────────────────────────────────────┘
```

---

## Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| C# / ASP.NET Core 8 | REST API framework |
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
| Docker | Multi-stage backend containerization |

### Testing
| Technology | Purpose |
|---|---|
| xUnit | Unit testing framework |
| EF Core InMemory | In-memory database for test isolation |
| Moq | Dependency mocking |
| FluentAssertions | Expressive test assertions |

---

## Object-Oriented Design

The backend demonstrates all three core OOP principles through deliberate design decisions, not incidental structure:

**Inheritance** — `BaseEntity` is an abstract parent class providing shared audit fields (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) to every model. `FinancialRecord`, `Report`, and `Transaction` all inherit from it without redefining those fields. `ApplicationUser` extends ASP.NET Identity's `IdentityUser` to add application-specific profile fields.

**Polymorphism** — All three model classes override the `GetSummary()` method defined as `virtual` in `BaseEntity`, each returning a summary string appropriate to its own data type. `OnBeforeSave()` is similarly overridden in `FinancialRecord` to normalize data before persistence.

**Encapsulation** — `FinancialRecord` uses a private backing field for `Amount`. The public setter enforces a business rule directly — any attempt to set a negative amount throws an `ArgumentException` — keeping validation logic behind a clean public interface rather than trusting callers to self-police.

---

## Project Structure

```
neciai/
├── src/
│   ├── backend/
│   │   └── NeciAI.API/
│   │       ├── Controllers/       # AuthController, FinancialRecordsController, ReportsController
│   │       ├── Data/               # NeciAIDbContext, DbSeeder
│   │       ├── DTOs/               # Request and response data transfer objects
│   │       ├── Interfaces/         # IFinancialService, IReportService
│   │       ├── Migrations/         # EF Core database migrations
│   │       ├── Models/             # BaseEntity, ApplicationUser, FinancialRecord, Report, Transaction
│   │       ├── Services/           # FinancialService, ReportService
│   │       ├── Dockerfile          # Multi-stage Docker build
│   │       └── Program.cs         # Application configuration and DI registration
│   └── frontend/
│       └── neciai-dashboard/
│           ├── src/
│           │   ├── components/    # Layout with responsive sidebar
│           │   ├── context/        # AuthContext with JWT management
│           │   ├── pages/          # Dashboard, Records, Reports, Search, Login
│           │   └── services/       # Axios API client
│           └── vercel.json        # SPA routing configuration
├── tests/
│   └── backend-tests/
│       └── NeciAI.Tests/
│           └── FinancialRecordTests.cs  # 14 unit tests
└── docs/
    └── diagrams/
        ├── neciai uml.png          # UML class diagram
        └── neciai wireframe.png    # Low-fidelity wireframe
```

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL (or a free [Neon](https://neon.tech) account)

### Backend Setup
```bash
cd src/backend/NeciAI.API

dotnet restore

# Set ConnectionStrings:DefaultConnection in appsettings.Development.json
# or via environment variable — never commit real credentials

dotnet ef database update
dotnet run   # available at http://localhost:5005
```

### Frontend Setup
```bash
cd src/frontend/neciai-dashboard

npm install
npm run dev   # available at http://localhost:5173
```

### Running Tests
```bash
cd tests/backend-tests/NeciAI.Tests
dotnet test --verbosity normal
```
```
Test summary: total: 14, failed: 0, succeeded: 14, skipped: 0, duration: 4.3s
```

---

## API Reference

Full interactive documentation: https://neciai-production.up.railway.app/swagger

| Group | Endpoints |
|---|---|
| **Auth** | `POST /api/Auth/register`, `POST /api/Auth/login`, `GET /api/Auth/profile` |
| **Financial Records** | Full CRUD + search + category filter + date range filter |
| **Reports** | Generate PDF, Generate Excel, Download, Delete |

All endpoints except `register` and `login` require a valid JWT Bearer token.

---

## Deployment

Deployed independently across three cloud platforms:

| Layer | Platform | Details |
|---|---|---|
| Frontend | Vercel | Global CDN, SPA routing via `vercel.json` |
| Backend | Railway | Docker container, multi-stage build, environment-variable configuration |
| Database | Neon | Serverless PostgreSQL, SSL-enforced connections, EF Core migrations |

**Docker:** the backend uses a multi-stage build — the .NET 8 SDK image compiles and publishes the app, and the lightweight ASP.NET Core 8 runtime image runs the compiled output, keeping the production image lean.

---

## Unit Tests

14 tests cover the core business logic:

| Category | Coverage |
|---|---|
| Encapsulation | Negative amount rejected, positive amount accepted |
| Polymorphism | `GetSummary()` returns class-specific output |
| Inheritance | Child classes correctly inherit `BaseEntity` fields |
| CRUD | Create, Read (all), Update, Delete (soft) |
| Search | Multiple rows returned for keyword match |
| Security | Unauthorized user receives null, not another user's record |
| Date Range | Records filtered correctly by start and end date |
| OnBeforeSave | Whitespace trimmed before persistence |

---

## About the Author

**Abraham Macias** — Software Engineer, B.S. Software Engineering (Western Governors University), beginning an M.S. in Software Engineering with an AI Engineering specialization in October 2026.

[![GitHub](https://img.shields.io/badge/GitHub-MrMustSeeUs-181717?style=flat&logo=github)](https://github.com/MrMustSeeUs)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0A66C2?style=flat&logo=linkedin)](https://www.linkedin.com/in/YOUR-LINKEDIN-HANDLE)
[![Portfolio](https://img.shields.io/badge/Portfolio-Teocalli%20Devs-1a1a1a?style=flat)](https://YOUR-TEOCALLI-DEVS-DOMAIN.com)

---

## License

This project is open source under the [MIT License](LICENSE).
