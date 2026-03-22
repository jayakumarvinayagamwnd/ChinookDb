# Chinook DDD API

A RESTful Web API built on the [Chinook](https://github.com/lerocha/chinook-database) music store sample database, demonstrating **Vertical Slice Architecture** and **Domain-Driven Design** principles with modern .NET patterns.

---

## Overview

The Chinook API models a digital music store, organizing business capabilities into clearly bounded domains. Each domain slice owns its own request models, handlers, validation, mapping, and endpoint wiring — keeping concerns isolated and dependencies explicit.


### Bounded Contexts

| Domain | Scope |
|---|---|
| **Catalog** | Artists, Albums, Tracks, Genres, Media Types |
| **Playlists** | Playlists and their Track associations |
| **Customers** | Customer profiles and contact management |
| **Billing** | Invoices and Invoice Line Items |
| **Employees** | Employee records and reporting hierarchy |
| **Analytics** | Read-only projections and reporting |

---

## Technology Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Language | C# 13 |
| Framework | ASP.NET Core 10 (Minimal APIs + Controllers) |
| Database | SQLite |
| ORM | Entity Framework Core 10 |
| API Documentation | OpenAPI + Scalar UI |
| Health Monitoring | ASP.NET Core Health Checks + Custom Dashboard |
| Logging | Serilog |
| Caching | Redis via IDistributedCache |
| Result Handling | FluentResults |

---

## Architecture

### Vertical Slice Architecture

Features are organized by **domain slice** rather than technical layer. Each slice (e.g. `Features/Catalog/GetById/`) contains everything needed for that use case:

- Query / Command record
- Request handler
- FluentValidation validator
- AutoMapper profile (shared per domain)
- Endpoint registration

### Patterns Applied

| Pattern | Implementation |
|---|---|
| **CQRS** | Separate result-based query/command contracts backed by MediatR |
| **Mediator** | MediatR dispatches all queries and commands; endpoints stay thin |
| **Pipeline Behaviors** | `ValidationBehavior<TRequest, TResponse>` runs FluentValidation and `CachingBehavior<TRequest, TResponse>` provides transparent cache-aside for cacheable queries |
| **Result Pattern** | FluentResults `Result<T>` is used across handlers for explicit success/failure flow |
| **Distributed Caching** | Redis cache-aside with per-query cache keys and expirations via `ICacheableQuery` |
| **Object Mapping** | AutoMapper with `ProjectTo<T>()` for efficient EF Core projections |
| **Problem Details** | RFC 7807 compliant error responses via `AddProblemDetails()` |
| **Global Exception Handling** | `ValidationExceptionHandler` maps FluentValidation failures to `400 Bad Request` |
| **Health Monitoring** | Liveness/readiness endpoints with custom checks for process memory, SQLite file + DbContext connectivity, and Redis round-trip probes |
| **Structured Logging** | Serilog with enriched context properties, console and rolling-file sinks |

---

## NuGet Packages

### ASP.NET Core & Entity Framework
| Package | Version |
|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.5 |
| `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | 10.0.5 |
| `Microsoft.EntityFrameworkCore` | 10.0.5 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.5 |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.5 |

### Mediator & CQRS
| Package | Version |
|---|---|
| `MediatR` | 11.1.0 |
| `MediatR.Extensions.Microsoft.DependencyInjection` | 11.1.0 |

### Mapping
| Package | Version |
|---|---|
| `AutoMapper` | 12.0.1 |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 12.0.1 |

### Validation
| Package | Version |
|---|---|
| `FluentValidation` | 12.1.1 |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 |

### Caching
| Package | Version |
|---|---|
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 10.0.0 |

### Result Pattern
| Package | Version |
|---|---|
| `FluentResults` | 3.15.0 |

### Logging
| Package | Version |
|---|---|
| `Serilog.AspNetCore` | 10.0.0 |
| `Serilog.Sinks.Console` | 6.1.1 |
| `Serilog.Sinks.File` | 7.0.0 |
| `Serilog.Settings.Configuration` | 10.0.0 |

### API Documentation
| Package | Version |
|---|---|
| `Scalar.AspNetCore` | 1.2.47 |

---

## Project Structure

```
src/
└── Chinook.API/
    ├── Common/
    │   ├── Behaviors/          # MediatR pipeline behaviors (ValidationBehavior, CachingBehavior)
    │   ├── Contracts/
    │   │   ├── Commands/       # IResultCommand<T>, IResultCommandHandler<T>
    │   │   └── Queries/        # IResultQuery<T>, IResultQueryHandler<T>
    │   ├── DependencyInjection/ # Service registration extensions
    │   ├── Results/            # FluentResults helpers and HTTP mapping extensions
    │   └── Exceptions/         # Global exception handlers (ValidationExceptionHandler)
    ├── Data/                   # Database seed data / scripts
    ├── Features/
    │   ├── Analytics/          # Read-only projections and reporting
    │   │   ├── GetById/
    │   │   ├── List/
    │   │   ├── Shared/
    │   │   └── AnalyticsEndpointExtensions.cs
    │   ├── Billing/            # Invoices and checkout flows
    │   │   ├── Create/
    │   │   ├── Delete/
    │   │   ├── GetById/
    │   │   ├── List/
    │   │   ├── Update/
    │   │   ├── Shared/
    │   │   └── BillingEndpointExtensions.cs
    │   ├── Catalog/            # Artists, Albums, Tracks, Genres, Media Types
    │   │   ├── GetById/        # GetArtistByIdQuery + Validator
    │   │   ├── List/           # ListArtistsQuery
    │   │   └── Shared/         # ArtistDto, CatalogMappingProfile
    │   ├── Customers/          # Customer profiles and support rep operations
    │   │   ├── Create/
    │   │   ├── Delete/
    │   │   ├── GetById/
    │   │   ├── List/
    │   │   ├── Update/
    │   │   ├── Shared/
    │   │   └── CustomerEndpointExtensions.cs
    │   ├── Employees/          # Employee profiles, reports, manager hierarchy
    │   │   ├── Create/
    │   │   ├── GetById/
    │   │   ├── List/
    │   │   ├── Update/
    │   │   ├── Shared/
    │   │   └── EmployeeEndpointExtensions.cs
    │   ├── Health/             # Health checks, response formatting, dashboard endpoint
    │   │   ├── Checks/         # ProcessMemoryHealthCheck, SqliteFileHealthCheck, RedisRoundTripHealthCheck
    │   │   ├── Formatting/     # Custom JSON response formatter
    │   │   └── HealthEndpointExtensions.cs
    │   └── Playlists/          # Playlists and track associations
    │       ├── Create/
    │       ├── Delete/
    │       ├── GetById/
    │       ├── List/
    │       ├── Update/
    │       ├── Shared/
    │       └── PlaylistEndpointExtensions.cs
    ├── Infrastructure/
    │   ├── Caching/            # Redis cache DI registration
    │   ├── Persistence/
    │   │   ├── ChinookDbContext.cs
    │   │   ├── Configurations/ # EF Core entity configurations
    │   │   ├── Entities/       # Domain entity classes
    │   │   └── Migrations/     # EF Core migrations
    │   └── DependencyInjection/ # Infrastructure service registration
    ├── Logs/                   # Rolling log files (daily)
    ├── appsettings.json
    └── Program.cs
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the API

```bash
cd src/Chinook.API
dotnet run
```

The API starts on `http://localhost:5185` by default.

### API Documentation

Scalar UI is available in Development mode:

```
http://localhost:5185/scalar/v1
```

OpenAPI JSON document:

```
http://localhost:5185/openapi/v1.json
```

### Health Monitoring

The API exposes production-style health endpoints with tagged probes:

- `GET /health/live` → liveness checks (process-level health)
- `GET /health/ready` → readiness checks (SQLite + Redis dependencies)
- `GET /health` → all checks (aggregate view)
- `GET /health-ui` → custom operational dashboard (HTML)
- `GET /health-ui-api` → dashboard data feed (custom JSON)

Registered checks include:

- `ProcessMemoryHealthCheck` (degrades when memory threshold is exceeded)
- `SqliteFileHealthCheck` (verifies SQLite file presence + metadata)
- `AddDbContextCheck<ChinookDbContext>` (verifies EF Core SQLite connectivity)
- `RedisRoundTripHealthCheck` (set/get/remove probe key in Redis)

The health JSON payload includes:

- overall status
- total duration in milliseconds
- per-check entries (status, duration, description, exception, tags, data)

### Database

The API uses a SQLite database located at `Data/Chinook.db` (relative to the project directory), configured via `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ChinookDb": "Data Source=Data/Chinook.db",
    "Redis": "127.0.0.1:6379"
  }
}
```

### Caching Notes

- Query slices that implement `ICacheableQuery` are automatically cached by `CachingBehavior`.
- Cache entries include per-query keys and optional expiry values.
- Failed FluentResults responses are not cached.
- Legacy/stale cache payloads are evicted and refreshed automatically.

---

## Logging

Serilog writes structured logs to:

- **Console** — formatted output with timestamp and log level
- **File** — `Logs/log-{date}.txt`, daily rolling, 20 MB size limit, 30 files retained

---

## Roadmap

Planned feature slices aligned to the bounded contexts defined in [Chinook DDD API Endpoint.md](Chinook%20DDD%20API%20Endpoint.md):

- [x] Caching (cache-aside for read-heavy catalog endpoints via Redis)
- [x] Result pattern for standardized operation outcomes (FluentResults)
- [x] Full Billing domain (invoice query/command handlers)
- [x] Playlists domain endpoints
- [x] Customers domain endpoints
- [x] Employees domain endpoints
- [x] Analytics projections
- [ ] OpenTelemetry tracing and metrics
- [ ] API versioning
