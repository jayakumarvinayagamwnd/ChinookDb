
# Chinook DDD API Endpoint Catalog

## Introduction

This document defines a domain-driven API catalog for the Chinook platform, excluding the HR domain.

The API is grouped by bounded contexts so each business capability can evolve with minimal coupling:

- Catalog
- Playlist
- Customer
- Billing
- Employee Operations
- Analytics

### Design Goals

- Keep business logic inside clear domain boundaries.
- Prefer intention-revealing endpoints over generic CRUD-only patterns.
- Make cross-domain relationships explicit.
- Keep analytics read-only and projection-driven.

---

## Architecture And Technology Stack

### Platform

- Runtime: .NET 10 (ASP.NET Core)
- Language: C#
- API Style: RESTful Web API

### API Documentation And Testing

- OpenAPI document is mapped in development.
- Scalar UI is enabled in development via `MapScalarApiReference()`.
- Launch profile opens Scalar at `http://localhost:5185/scalar/v1`.

### Logging Configuration (Serilog)

- Console and rolling file logging are enabled.
- File logs are written to `Logs/log-.txt` (daily rolling, size limit, retention).
- Startup logs include quick access URL for Scalar.

### Architecture Style

1. Vertical Slice Architecture
	- Organize by feature slices (for example: Catalog, Playlist, Billing), not by technical layers alone.
	- Each slice owns its request models, handlers, validation, and endpoint wiring.
	- Keep dependencies inward and use shared abstractions only where necessary.

### Core Design Patterns And Libraries

2. MediatR
	- Use request/response and notification patterns for application use cases.
	- Keep controllers/endpoints thin by dispatching commands and queries through MediatR.

3. Result Pattern
	- Standardize operation outcomes using a Result type (Success/Failure, error code, message, and optional payload).
	- Avoid exception-driven flow for expected domain/application validation outcomes.

4. Serilog
	- Use structured logging with correlation identifiers and contextual properties.
	- Write logs to console and configurable sinks (file, Seq, or other observability backends).

5. Caching
	- Apply cache-aside for read-heavy endpoints (for example: genres, media types, top tracks, and catalog summaries).
	- Use in-memory cache for single-node scenarios and distributed cache (for example: Redis) for scale-out.
	- Define explicit cache keys, TTL, and invalidation rules per bounded context.

6. FluentValidation
	- Validate commands and queries using FluentValidation validators per feature slice.
	- Run validation in MediatR pipeline behaviors so invalid requests fail fast before handler execution.
	- Return consistent validation failures through the Result pattern and standardized API error responses.

### Additional Modern Principles And Patterns

7. CQRS (Pragmatic)
	- Separate read and write models where complexity or scale justifies it.
	- Keep simple CRUD flows straightforward; apply CQRS selectively per slice.

8. Clean Boundaries
	- Keep Domain free of infrastructure concerns.
	- Use Application layer orchestration with interfaces for persistence, messaging, and external services.

9. Global Error Handling + Problem Details
	- Use centralized exception handling middleware.
	- Emit RFC 7807 Problem Details responses for predictable client integration.

10. Observability By Default
	- Add OpenTelemetry tracing, metrics, and log correlation.
	- Track request latency, failure rates, cache hit ratio, and DB query timing.

11. Resilience Patterns
	- Use Polly policies for retry, timeout, circuit breaker, and fallback around external dependencies.
	- Apply idempotency for critical write operations (for example: invoice finalize/void).

12. API-First Contract And Versioning
	- Generate and maintain OpenAPI/Swagger contracts as part of CI.
	- Use explicit versioning and deprecation policy to protect consumers.

13. Security Baseline
	- Enforce authentication/authorization policies per endpoint.
	- Validate input size/range limits and apply secure defaults for headers and transport.

14. Testing Strategy
	- Unit test domain logic and handlers.
	- Add integration tests for API + database behavior.
	- Add contract tests to keep API implementation aligned with OpenAPI.

---

## Chinook.API Folder Design And Usage

### Target Project Structure (Vertical Slice)

- `src/Chinook.API/Program.cs`
- `src/Chinook.API/appsettings.json`
- `src/Chinook.API/Data/`
- `src/Chinook.API/Common/`
- `src/Chinook.API/Infrastructure/`
- `src/Chinook.API/Features/`

### Folder Responsibilities

#### Program.cs

- Composition root for the API.
- Register MediatR, FluentValidation, Serilog, caching, and infrastructure services.
- Map feature endpoints and middleware pipeline.

#### Data/

- Hosts runtime data files used by the API.
- Store the SQLite file as `Data/Chinook.db`.
- Keep this folder for local/dev data artifacts, not domain code.

#### Common/

- Cross-cutting building blocks reused by all slices.
- `Common/Behaviors`: MediatR pipeline behaviors (validation, logging, performance).
- `Common/Results`: Result pattern primitives and error models.
- `Common/Validation`: shared validation helpers.
- `Common/Caching`: cache key and cache policy abstractions.
- `Common/Contracts`: shared request/response contracts only when truly common.
- `Common/Errors`: shared application/domain error definitions.
- `Common/Logging`: logging helpers/enrichers.

#### Infrastructure/

- External integration implementations.
- `Infrastructure/Persistence`: DbContext, repository/query implementations, DB access.
- `Infrastructure/Caching`: memory/redis cache provider implementations.
- `Infrastructure/Logging`: Serilog sink/enricher wiring.
- `Infrastructure/DependencyInjection`: service registration extensions.

#### Features/

- Main Vertical Slice area, organized by business capability.
- Each feature owns commands, queries, handlers, validators, endpoint maps, and DTOs.
- Example slices:
	- `Features/Catalog/...`
	- `Features/Playlists/...`
	- `Features/Customers/...`
	- `Features/Billing/Invoices/...`
	- `Features/Employees/...`
	- `Features/Analytics/...`

### How To Add A New Feature Slice

1. Create a feature folder under `Features/<Domain>/<UseCase>`.
2. Add `Command` or `Query` model.
3. Add `Handler` implementing MediatR request handling.
4. Add `Validator` using FluentValidation.
5. Add endpoint mapping for the slice.
6. Return standardized Result objects.
7. Add tests for validator + handler + endpoint behavior.

### Folder Design Rules

- Keep business logic in feature handlers/domain services, not in Program.cs.
- Prefer duplication between slices over premature shared abstractions.
- Move code to `Common` only after a clear reuse pattern appears.
- Keep `Infrastructure` replaceable; depend on interfaces in feature code.
- Keep analytics slices read-only by default.

---

## API Conventions

- Base path: `/api`
- Versioning recommendation: `/api/v1/...`
- Resource IDs: `{id}` placeholders (integer or UUID by implementation)
- Read operations: `GET`
- Write operations: `POST`, `PATCH`, `DELETE`

---

## Chinook Database Reference

- Database name: `Chinook`
- Default schema: `dbo`

### Core Tables

- `artists`
- `albums`
- `tracks`
- `genres`
- `media_types`
- `playlists`
- `playlist_track`
- `customers`
- `employees`
- `invoices`
- `invoice_items`

### Entity Classes (Short Summary)

- `Infrastructure/Persistence/Entities/Catalog/Artist.cs`: Artist master record and album ownership.
- `Infrastructure/Persistence/Entities/Catalog/Album.cs`: Album metadata linked to one artist and many tracks.
- `Infrastructure/Persistence/Entities/Catalog/Track.cs`: Track metadata with album, genre, and media type references.
- `Infrastructure/Persistence/Entities/Catalog/Genre.cs`: Genre lookup for track classification.
- `Infrastructure/Persistence/Entities/Catalog/MediaType.cs`: Media format lookup for tracks.

- `Infrastructure/Persistence/Entities/Playlists/Playlist.cs`: Playlist aggregate root containing playlist-track links.
- `Infrastructure/Persistence/Entities/Playlists/PlaylistTrack.cs`: Join entity for many-to-many Playlist and Track mapping.

- `Infrastructure/Persistence/Entities/Customers/Customer.cs`: Customer profile and support rep relationship.

- `Infrastructure/Persistence/Entities/Billing/Invoice.cs`: Invoice header with billing details and total amount.
- `Infrastructure/Persistence/Entities/Billing/InvoiceLine.cs`: Invoice line item linking invoice to purchased track.

- `Infrastructure/Persistence/Entities/Employees/Employee.cs`: Employee profile with manager hierarchy and supported customers.

- `Infrastructure/Persistence/Entities/Analytics/`: Reserved for read-model entities/projections used by analytics endpoints.

---

## 1. Catalog Domain

### Core Purpose
Manage music metadata: artists, albums, tracks, genres, and media types.

### Endpoints

- `GET /api/catalog/artists`
- `GET /api/catalog/artists/{artistId}`
- `POST /api/catalog/artists`
- `PATCH /api/catalog/artists/{artistId}`
- `DELETE /api/catalog/artists/{artistId}`
- `GET /api/catalog/artists/{artistId}/albums`

- `GET /api/catalog/albums`
- `GET /api/catalog/albums/{albumId}`
- `POST /api/catalog/albums`
- `PATCH /api/catalog/albums/{albumId}`
- `DELETE /api/catalog/albums/{albumId}`
- `GET /api/catalog/albums/{albumId}/tracks`

- `GET /api/catalog/tracks`
- `GET /api/catalog/tracks/{trackId}`
- `POST /api/catalog/tracks`
- `PATCH /api/catalog/tracks/{trackId}`
- `DELETE /api/catalog/tracks/{trackId}`

- `GET /api/catalog/genres`
- `GET /api/catalog/media-types`

### Related Domain-Specific Endpoints

- `POST /api/catalog/albums/{albumId}/publish`
- `POST /api/catalog/tracks/{trackId}/reclassify-genre`
- `GET /api/catalog/search?q={term}&type=artist,album,track`

---

## 2. Playlist Domain

### Core Purpose
Manage playlist lifecycle and playlist-track relationships.

### Endpoints

- `GET /api/playlists`
- `GET /api/playlists/{playlistId}`
- `POST /api/playlists`
- `PATCH /api/playlists/{playlistId}`
- `DELETE /api/playlists/{playlistId}`
- `GET /api/playlists/{playlistId}/tracks`
- `POST /api/playlists/{playlistId}/tracks`
- `DELETE /api/playlists/{playlistId}/tracks/{trackId}`

### Related Domain-Specific Endpoints

- `POST /api/playlists/{playlistId}/reorder`
- `POST /api/playlists/{playlistId}/clone`
- `GET /api/playlists/{playlistId}/recommendations`

---

## 3. Customer Domain

### Core Purpose
Manage customer identity, profile information, support assignment, and customer preferences.

### Endpoints

- `GET /api/customers`
- `GET /api/customers/{customerId}`
- `POST /api/customers`
- `PATCH /api/customers/{customerId}`
- `DELETE /api/customers/{customerId}`
- `GET /api/customers/{customerId}/support-rep`
- `PATCH /api/customers/{customerId}/support-rep`

### Related Domain-Specific Endpoints

- `PATCH /api/customers/{customerId}/address`
- `PATCH /api/customers/{customerId}/contact-preferences`
- `GET /api/customers/{customerId}/purchase-history`

---

## 4. Billing Domain

### Core Purpose
Handle invoices, invoice lines, checkout, and billing totals.

### Endpoints

- `GET /api/billing/invoices`
- `GET /api/billing/invoices/{invoiceId}`
- `POST /api/billing/invoices`
- `POST /api/billing/invoices/{invoiceId}/lines`
- `DELETE /api/billing/invoices/{invoiceId}/lines/{lineId}`
- `POST /api/billing/invoices/{invoiceId}/finalize`
- `POST /api/billing/invoices/{invoiceId}/void`
- `GET /api/billing/customers/{customerId}/invoices`

### Related Domain-Specific Endpoints

- `POST /api/billing/checkout`
- `GET /api/billing/invoices/{invoiceId}/totals`
- `GET /api/billing/revenue?from=YYYY-MM-DD&to=YYYY-MM-DD`

---

## 5. Employee Operations Domain

### Core Purpose
Manage operational employee responsibilities related to customer support and reporting structures.

### Endpoints

- `GET /api/employees`
- `GET /api/employees/{employeeId}`
- `POST /api/employees`
- `PATCH /api/employees/{employeeId}`
- `GET /api/employees/{employeeId}/reports`
- `GET /api/employees/{employeeId}/customers`

### Related Domain-Specific Endpoints

- `PATCH /api/employees/{employeeId}/manager`
- `GET /api/employees/hierarchy`

---

## 6. Analytics Domain (Read-Only)

### Core Purpose
Provide reporting and analytics views based on read projections from operational domains.

### Endpoints

- `GET /api/analytics/top-tracks`
- `GET /api/analytics/top-artists`
- `GET /api/analytics/revenue-by-country`
- `GET /api/analytics/customer-ltv/{customerId}`
- `GET /api/analytics/sales-trend?interval=day|month|year`

### Related Domain-Specific Endpoints

- `GET /api/analytics/cohort-retention`
- `GET /api/analytics/playlist-engagement/{playlistId}`

---

## Cross-Domain Relationships

- Catalog Track -> Billing InvoiceLine
- Customer -> Billing Invoice
- Employee Operations -> Customer support assignment
- Playlist -> Catalog Track reference
- Analytics -> Read projections from Catalog, Playlist, Customer, Billing, Employee Operations

---

## Suggested Domain Events

- `AlbumPublished`
- `TrackGenreReclassified`
- `TrackAddedToPlaylist`
- `InvoiceFinalized`
- `InvoiceVoided`
- `SupportRepAssignedToCustomer`

---

## Summary

This combined endpoint catalog applies DDD principles for Chinook while excluding HR from scope.

- API boundaries are aligned to business capabilities.
- Domain-specific operations are explicit and intention-driven.
- Analytics remains read-only and optimized for reporting.
- Cross-domain relationships are clear and can be implemented via events or application services.

This catalog is ready to be transformed into an OpenAPI contract and implemented through layered architecture (Domain, Application, Infrastructure, API).

fluent validation, fluent result pattern, endpoint result header, 

implement POST /api/catalog/artists
with same verticle-slice pattern as 
(Query + Handler + automapper projection + fluent validation, Fluent Results )

