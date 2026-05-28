## IronCore Gym SaaS — Backend API

A multi-tenant Gym management REST API built with **.NET 9 / ASP.NET Core** following **Clean Architecture** principles.

### Architecture

The solution is organized into six projects:

| Project | Responsibility |
|---|---|
| `GymSaaS.API` | HTTP controllers, middleware pipeline, DI registration |
| `GymSaaS.Application` | Service layer — business logic, DTOs, interfaces |
| `GymSaaS.Domain` | Entities, enums, domain interfaces |
| `GymSaaS.Persistence` | EF Core `DbContext`, repositories, migrations |
| `GymSaaS.Infrastructure` | External integrations (Auth, Cloudinary) |
| `GymSaaS.Shared` | Cross-cutting helpers: `ApiResponse<T>`, constants, permission keys |

### Tech Stack

- **Runtime**: .NET 9
- **ORM**: Entity Framework Core 9 + PostgreSQL (Npgsql)
- **Auth**: JWT Bearer — HMACSHA256, 8-hour expiry, role claims (`Owner`, `Manager`, `Receptionist`, `Trainer`)
- **Multi-tenancy**: Shared database with row-level isolation via EF global query filters on all tenant-scoped entities
- **Image storage**: Cloudinary

### Key Modules

- **Auth** — Tenant registration (creates Tenant + Owner atomically), login, JWT issuance
- **Members** — Full CRUD, profile photos via Cloudinary, branch/trainer assignment
- **Trainers** — CRUD with specialization, certifications, availability
- **Memberships & Packages** — Enrollment, renewal, pricing tiers (Monthly / Full Payment)
- **Payments** — Auto-generated monthly payment schedules, late-status tracking (7-day grace period), payment recording, backfill for existing memberships
- **Attendance** — Check-in / check-out logging per branch
- **Branches** — Multi-location management
- **Settings** — Working hours, class types, service configuration

### Running Locally

```bash
# Prerequisites: .NET 9 SDK, PostgreSQL running on localhost:5432

cd GymSaaS.Backend
dotnet restore
dotnet run --project src/GymSaaS.API
# API available at http://localhost:5123/api
```

Configure the database connection in `src/GymSaaS.API/appsettings.Development.json`.
