# CarsInfo API

A versioned REST API for cars and their points of interest, built with **ASP.NET Core 8**, **EF Core** and **JWT authentication**.

## Features

- **RESTful resources**: full CRUD for points of interest, including partial updates with JSON Patch (`PATCH`).
- **Filtering, searching and paging**: filter cars by name, search across name and description, and page through results. Paging metadata is returned in an `X-Pagination` response header.
- **JWT authentication**: tokens are issued by the API and validated on every request.
- **Resource-based authorization**: a token carries a `car` claim, and point-of-interest endpoints return `403 Forbidden` for cars the caller doesn't own.
- **API versioning**: the version is part of the URL (`/api/v1/...`, `/api/v2/...`), supported and deprecated versions are reported in response headers, and Swagger has a separate document per version.
- **OpenAPI / Swagger UI**: generated from XML comments, with response types, status codes and Bearer authentication.
- **Content negotiation**: JSON by default, XML when the client sends `Accept: application/xml`.
- **EF Core with SQLite**: code-first migrations and seed data, accessed through a repository with async queries.
- **Compile-time object mapping** with [Mapperly](https://mapperly.riok.app/): no runtime reflection.
- **Structured logging** with Serilog to the console and to rolling log files.
- **Production concerns**: RFC 7807 problem details, global exception handling, forwarded headers for running behind a reverse proxy.

## Tech stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core 8 (controllers) |
| Data access | Entity Framework Core 8, SQLite |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| API versioning | `Asp.Versioning.Mvc` |
| Documentation | Swashbuckle (Swagger / OpenAPI) |
| Mapping | Riok.Mapperly (source generator) |
| Logging | Serilog |

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Run locally

```bash
git clone https://github.com/isgandarovmahammad4/CarsInfo.git
cd CarsInfo
```

**1. Set the JWT signing key.** The key is not stored in the repository; for local development it lives in [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets). Generate a random 256-bit key and store it:

```powershell
# PowerShell (Windows PowerShell 5.1 and PowerShell 7)
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Authentication:SecretForKey" ([Convert]::ToBase64String($bytes)) --project CarsInfo.Api
```

```bash
# bash
dotnet user-secrets set "Authentication:SecretForKey" "$(openssl rand -base64 32)" --project CarsInfo.Api
```

The API refuses to start with a clear error message if this key is missing.

**2. Create the database** from the migrations (this also seeds sample data):

```bash
dotnet ef database update --project CarsInfo.Api
```

**3. Run the API:**

```bash
dotnet run --project CarsInfo.Api --launch-profile https
```

Swagger UI is available at **https://localhost:7098/swagger**. Use the dropdown in the top-right corner to switch between API versions.

## Using the API

### 1. Get a token

```http
POST /api/authentication/authenticate
Content-Type: application/json

{ "username": "demo", "password": "demo" }
```

The response body is the JWT. It is valid for one hour and grants access to **Car 1**.

> Authentication is a demo: any username and password are accepted. See [Known limitations](#known-limitations).

### 2. Call an endpoint

In Swagger UI, click **Authorize** and paste the token. From any other client, send it as a header:

```http
GET /api/v1/cars?searchQuery=car&pageNumber=1&pageSize=10
Authorization: Bearer <token>
```

### Endpoints

All endpoints except `authenticate` require a token.

| Version | Method | Route | Description |
|---|---|---|---|
| – | `POST` | `/api/authentication/authenticate` | Issue a JWT |
| 1.0 | `GET` | `/api/v1/cars` | List cars. Query: `name` (exact match), `searchQuery` (name or description), `pageNumber`, `pageSize` (max 20) |
| 1.0 | `GET` | `/api/v1/cars/{carId}` | Get a car. Query: `includePoi=true` to include its points of interest |
| 2.0 | `GET` | `/api/v2/cars/{carId}/pointsofinterest` | List a car's points of interest |
| 2.0 | `GET` | `/api/v2/cars/{carId}/pointsofinterest/{id}` | Get a point of interest |
| 2.0 | `POST` | `/api/v2/cars/{carId}/pointsofinterest` | Create a point of interest |
| 2.0 | `PUT` | `/api/v2/cars/{carId}/pointsofinterest/{id}` | Replace a point of interest |
| 2.0 | `PATCH` | `/api/v2/cars/{carId}/pointsofinterest/{id}` | Partially update a point of interest (JSON Patch) |
| 2.0 | `DELETE` | `/api/v2/cars/{carId}/pointsofinterest/{id}` | Delete a point of interest |
| 0.1 | `GET` | `/api/v0.1/files/{fileId}` | Download a sample file (**deprecated** version) |

### Status codes worth knowing

| Code | Meaning in this API |
|---|---|
| `401 Unauthorized` | No token, an expired token, or a token signed with the wrong key |
| `403 Forbidden` | Valid token, but the `car` claim doesn't match the requested car |
| `404 Not Found` | The car or point of interest doesn't exist |

## Project structure

```
CarsInfo.Api/
├── Controllers/    HTTP endpoints (cars, points of interest, files, authentication)
├── DbContexts/     EF Core DbContext and seed data
├── Entities/       Database entities
├── Models/         DTOs returned by and sent to the API
├── Mapping/        Mapperly mappers between entities and DTOs
├── Services/       Repository and mail service implementations
│   └── Abstraction/  Their interfaces
├── Swagger/        Per-version Swagger document configuration
├── Migrations/     EF Core migrations
└── Program.cs      Service registration and HTTP pipeline
```

## Design decisions

- **Version in the URL segment** (`/api/v1/cars`) rather than a header: the version is visible in logs, links and browser address bars, and each version gets its own Swagger document.
- **Mapperly instead of AutoMapper**: AutoMapper 12 has a known high-severity vulnerability, and the fixed versions require a commercial license. Mapperly generates plain C# at compile time, so a missing or misspelled mapping is reported by the build instead of failing silently at runtime.
- **Ownership check in the controller** (`CanAccessCar`): the decision depends on the requested resource, which a static policy on an attribute can't see. The roadmap moves this into a resource-based `IAuthorizationHandler`.
- **Secrets outside the repository**: the signing key comes from user secrets locally and is meant to come from environment variables or a key vault when deployed. Missing configuration fails at startup, not on the first request.
- **XML documentation without `CS1591`**: XML comments exist to document the API in Swagger, not every DTO property, so only that warning is suppressed. The build has zero warnings.

## Known limitations

This project is a learning project and is not production-ready yet:

- **Demo authentication**: there is no user store; any credentials receive a token for Car 1.
- **SQLite**: fine for local development, not for concurrent production load.
- **No automated tests yet.**

## Roadmap

- [ ] Unit and integration tests (xUnit, `WebApplicationFactory`)
- [ ] Real users with hashed passwords, roles and refresh tokens
- [ ] Resource-based authorization handler
- [ ] Docker and Docker Compose with PostgreSQL
- [ ] CI pipeline with GitHub Actions
- [ ] Health checks and rate limiting

## Acknowledgements

This project started from Kevin Dockx's *ASP.NET Core Web API Fundamentals* course and has since been extended and refactored beyond it.
