# LogiFlow

[![CI](https://github.com/IvRom201/LogiFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/IvRom201/LogiFlow/actions/workflows/ci.yml)

LogiFlow is a fullstack logistics management MVP built with **.NET 9**, **Angular 18**, **PostgreSQL**, and **Docker**.

The application allows a dispatcher to sign in, view active logistics trips, create new trips, and manage the trip lifecycle by completing or cancelling active trips.

The main backend feature is transactional trip creation. Creating a trip is not a simple CRUD operation: the system validates cargo, vehicle, and driver availability, creates the trip, and updates all related statuses consistently in one database transaction.

---

## Features

- Dispatcher login with JWT-based authentication
- Active trips dashboard
- Debounced trip search
- Trip creation form
- Vehicle availability validation
- Complete trip action
- Cancel trip action
- PostgreSQL persistence
- EF Core migrations
- Dockerized backend, frontend, and database
- Backend unit tests
- GitHub Actions CI pipeline

---

## Tech Stack

### Backend

- .NET 9 Web API
- Minimal APIs with `MapGroup`
- Clean Architecture
- MediatR
- Entity Framework Core
- PostgreSQL
- EF Core Migrations
- Fluent API entity configuration
- Repository abstractions
- Unit of Work
- Explicit transaction handling
- JWT Bearer authentication
- Swagger / OpenAPI
- Global exception handling
- xUnit, Moq, FluentAssertions

### Frontend

- Angular 18
- Standalone Components
- Angular Signals
- RxJS
- Angular Material
- Reactive Forms
- Functional HTTP Interceptors
- JWT-based login flow
- Protected routes with route guard

### DevOps

- Docker
- Docker Compose
- PostgreSQL container
- Nginx for Angular production hosting
- GitHub Actions CI

---

## Project Structure

```text
LogiFlow/
├── Backend/
│   └── LodiFlowBackend/
│       ├── LogiFlow.Domain/
│       ├── LogiFlow.Application/
│       ├── LogiFlow.Infrastructure/
│       ├── LogiFlow.WebApi/
│       ├── LogiFlow.Domain.Tests/
│       ├── LogiFlow.Application.Tests/
│       └── LodiFlowBackend.sln
│
├── Frontend/
│   ├── src/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── proxy.conf.json
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── docker-compose.yml
└── README.md
```

---

## Architecture

The backend follows Clean Architecture principles.

```text
WebApi
  ↓
Application
  ↓
Domain

Infrastructure implements Application abstractions.
```

### Domain Layer

Contains the core business model:

- `Cargo`
- `Vehicle`
- `Driver`
- `Trip`

And status enums:

- `CargoStatus`
- `VehicleStatus`
- `DriverStatus`
- `TripStatus`

The domain entities contain business rules and state transitions.

Examples:

- cargo can be assigned only when it is pending;
- vehicle can be assigned only when it is idle and has enough capacity;
- driver can be assigned only when available;
- completed trips cannot be cancelled;
- cancelled trips cannot be completed.

### Application Layer

Contains use cases implemented with MediatR:

- `CreateTripCommand`
- `CompleteTripCommand`
- `CancelTripCommand`
- `GetActiveTripsQuery`
- `CheckVehicleAvailabilityQuery`

It also contains:

- DTOs;
- repository interfaces;
- Unit of Work abstraction;
- transaction abstraction;
- application-level exceptions.

### Infrastructure Layer

Contains technical implementations:

- EF Core `AppDbContext`;
- PostgreSQL configuration;
- Fluent API entity configurations;
- repository implementations;
- EF Core migrations;
- database seeding;
- transaction implementation.

### WebApi Layer

Contains:

- Minimal API endpoints;
- Swagger configuration;
- JWT Bearer authentication;
- demo authentication endpoints;
- global exception handling;
- CORS configuration for the Angular frontend.

---

## Main Business Flow

### Creating a Trip

```text
POST /api/trips
        ↓
CreateTripCommand
        ↓
CreateTripCommandHandler
        ↓
Begin database transaction
        ↓
Lock cargo FOR UPDATE
Lock vehicle FOR UPDATE
Lock driver FOR UPDATE
        ↓
Validate cargo / vehicle / driver availability
        ↓
Create Trip
        ↓
Update statuses:
- cargo   -> Assigned
- vehicle -> Busy
- driver  -> OnTrip
        ↓
SaveChanges
        ↓
Commit transaction
```

This prevents assigning the same cargo, vehicle, or driver to multiple active trips at the same time.

### Completing a Trip

```text
PUT /api/trips/{id}/complete
        ↓
CompleteTripCommand
        ↓
CompleteTripCommandHandler
        ↓
Update statuses:
- trip    -> Completed
- cargo   -> Delivered
- vehicle -> Idle
- driver  -> Available
```

### Cancelling a Trip

```text
PUT /api/trips/{id}/cancel
        ↓
CancelTripCommand
        ↓
CancelTripCommandHandler
        ↓
Update statuses:
- trip    -> Cancelled
- cargo   -> Cancelled
- vehicle -> Idle
- driver  -> Available
```

---

## Backend API

Most business endpoints require JWT authentication.

### Auth

#### Login

```http
POST /api/auth/login
```

Example request:

```json
{
  "email": "dispatcher@logiflow.local",
  "password": "Dispatcher123!"
}
```

Example response:

```json
{
  "accessToken": "jwt-token",
  "email": "dispatcher@logiflow.local",
  "fullName": "Demo Dispatcher",
  "role": "Dispatcher",
  "expiresAt": "2026-05-15T12:00:00Z"
}
```

#### Get Current User

```http
GET /api/auth/me
```

Requires authorization.

---

### Trips

All trip endpoints require authorization.

#### Get Active Trips

```http
GET /api/trips/active
```

Optional search parameter:

```http
GET /api/trips/active?search=warsaw
```

Returns active trips. Search can match route, cargo, vehicle, or driver data.

#### Create Trip

```http
POST /api/trips
```

Example request:

```json
{
  "cargoId": "00000000-0000-0000-0000-000000000000",
  "vehicleId": "00000000-0000-0000-0000-000000000000",
  "driverId": "00000000-0000-0000-0000-000000000000",
  "origin": "Warsaw",
  "destination": "Berlin",
  "scheduledStart": "2026-05-14T10:00:00Z",
  "scheduledEnd": "2026-05-14T18:00:00Z"
}
```

Possible responses:

| Status | Description |
|---|---|
| `201 Created` | Trip was created successfully |
| `400 Bad Request` | Invalid input data |
| `404 Not Found` | Cargo, vehicle, or driver was not found |
| `409 Conflict` | Cargo, vehicle, or driver is not available |

#### Complete Trip

```http
PUT /api/trips/{id}/complete
```

Completes an active trip and releases assigned resources.

Possible responses:

| Status | Description |
|---|---|
| `200 OK` | Trip was completed successfully |
| `404 Not Found` | Trip was not found |
| `409 Conflict` | Trip cannot be completed in its current state |

#### Cancel Trip

```http
PUT /api/trips/{id}/cancel
```

Cancels an active trip and releases assigned resources.

Possible responses:

| Status | Description |
|---|---|
| `200 OK` | Trip was cancelled successfully |
| `404 Not Found` | Trip was not found |
| `409 Conflict` | Trip cannot be cancelled in its current state |

---

### Vehicles

Vehicle endpoints require authorization.

#### Check Vehicle Availability

```http
GET /api/vehicles/{id}/availability
```

Checks whether a vehicle is currently available.

---

## Demo Authentication

The project uses a simple demo authentication flow.

Demo users:

| Role | Email | Password |
|---|---|---|
| Dispatcher | `dispatcher@logiflow.local` | `Dispatcher123!` |
| Admin | `admin@logiflow.local` | `Admin123!` |

This is intentionally lightweight and suitable for a portfolio MVP.  
A production version should use hashed passwords, persistent users, refresh tokens, and stricter secret management.

---

## Local Development

### Requirements

- .NET 9 SDK
- Node.js 20+
- Angular CLI 18+
- Docker Desktop
- PostgreSQL via Docker

---

## Run with Docker

From the repository root:

```bash
docker compose up --build
```

Expected URLs:

| Service | URL |
|---|---|
| Frontend | `http://localhost:4200` |
| Backend Swagger | `http://localhost:8080/swagger` |
| PostgreSQL | `localhost:5432` |

PostgreSQL credentials:

| Property | Value |
|---|---|
| Host | `localhost` |
| Port | `5432` |
| Database | `logiflow` |
| Username | `postgres` |
| Password | `postgres` |

---

## Run PostgreSQL Only

If you want to run the backend locally through Rider, Visual Studio, or `dotnet run`, start only PostgreSQL first:

```bash
docker compose up -d postgres
```

---

## Run Backend Locally

Open the solution:

```text
Backend/LodiFlowBackend/LodiFlowBackend.sln
```

Or run from terminal:

```bash
cd Backend/LodiFlowBackend
dotnet run --project LogiFlow.WebApi
```

Swagger will be available at the URL printed in the console, for example:

```text
http://localhost:5081/swagger
```

Before starting the backend, PostgreSQL must be running because the application applies pending migrations on startup.

---

## Database Migrations

Create a new migration:

```bash
cd Backend/LodiFlowBackend

dotnet ef migrations add MigrationName --project LogiFlow.Infrastructure --startup-project LogiFlow.WebApi --context AppDbContext --output-dir Persistence/Migrations
```

Apply migrations manually:

```bash
dotnet ef database update --project LogiFlow.Infrastructure --startup-project LogiFlow.WebApi --context AppDbContext
```

The application also applies pending migrations automatically on startup.

---

## Run Backend Tests

From the backend solution folder:

```bash
cd Backend/LodiFlowBackend
dotnet test
```

The backend test projects cover:

- domain entity rules;
- state transitions;
- trip creation flow;
- trip completion flow;
- trip cancellation flow;
- transaction commit / rollback behavior.

---

## Run Frontend Locally

From the frontend folder:

```bash
cd Frontend
npm install
npm start
```

Frontend runs at:

```text
http://localhost:4200
```

The Angular dev server uses `proxy.conf.json` to forward `/api` requests to the backend.

Example `proxy.conf.json`:

```json
{
  "/api": {
    "target": "http://localhost:5081",
    "secure": false,
    "changeOrigin": true
  }
}
```

The target port must match the backend HTTP port printed in the backend console.

---

## Frontend Features

### Login

The frontend contains a login page with demo credentials.

After successful login:

- JWT access token is stored in local storage;
- authenticated user data is stored in local storage;
- protected routes become available;
- API requests automatically include the bearer token.

### Dashboard

The dashboard shows the main logistics workflow:

- active trips table;
- debounced search;
- loading state;
- error state;
- refresh action;
- trip creation form;
- vehicle availability validation;
- complete trip action;
- cancel trip action.

### HTTP Interceptors

The frontend includes:

- auth interceptor;
- global HTTP error interceptor.

The auth interceptor attaches the JWT bearer token to outgoing API requests.

The global error interceptor handles unauthorized responses and redirects to login when needed.

---

## Error Handling

The API uses global exception handling and returns consistent HTTP problem responses.

Typical mappings:

| Exception | HTTP Status |
|---|---|
| `BadRequestException` | `400 Bad Request` |
| `NotFoundException` | `404 Not Found` |
| `ConflictException` | `409 Conflict` |
| Unhandled exception | `500 Internal Server Error` |

Example conflict response:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "Driver is not available."
}
```

---

## CI

The repository includes GitHub Actions CI.

The CI pipeline runs on push and pull request to `main`.

Backend job:

```bash
dotnet restore
dotnet build
dotnet test
```

Frontend job:

```bash
npm ci
npm run build
```

This ensures that backend compilation, backend tests, and frontend production build are checked automatically.

---

## Notes

This is a portfolio MVP, not a production logistics platform.

Production improvements could include:

- persistent user management with ASP.NET Core Identity;
- password hashing and refresh tokens;
- role-based authorization policies;
- richer trip planning logic;
- integration tests with Testcontainers;
- frontend end-to-end tests;
- deployment configuration for cloud hosting.