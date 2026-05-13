# LogiFlow

LogiFlow is a logistics management MVP built with \*\*.NET 9\*\*, \*\*Angular 18\*\*, \*\*PostgreSQL\*\*, and \*\*Docker\*\*.

The application allows users to view active trips and create new trips by assigning available vehicles and drivers to cargo.



## Tech Stack


### Backend



- .NET 9 Web API

- Clean Architecture

- MediatR

- Entity Framework Core

- PostgreSQL

- Fluent API entity configuration

- Swagger / OpenAPI

- JWT Bearer authentication configuration



### Frontend



- Angular 18

- Standalone Components

- Angular Signals

- RxJS

- Angular Material

- Functional HTTP Interceptors



### DevOps



- Docker

- Docker Compose

- PostgreSQL container

- Nginx for Angular production hosting



---



## Project Structure


LogiFlow/

├── Backend/

│   └── LodiFlowBackend/

│       ├── LogiFlow.Domain/

│       ├── LogiFlow.Application/

│       ├── LogiFlow.Infrastructure/

│       ├── LogiFlow.WebApi/

│       ├── Dockerfile

│       └── .dockerignore

│

├── Frontend/

│   ├── src/

│   ├── Dockerfile

│   ├── nginx.conf

│   ├── proxy.conf.json

│   └── .dockerignore

│

├── docker-compose.yml

└── README.md



## Architecture

The backend follows Clean Architecture principles.

### Domain Layer

Contains core business entities and enums:

- `Cargo`
- `Vehicle`
- `Driver`
- `Trip`
- `VehicleStatus`
- `DriverStatus`
- `CargoStatus`
- `TripStatus`

### Application Layer

Contains application logic:

- DTOs
- MediatR commands and queries
- Repository abstractions
- Unit of Work abstraction
- Transaction abstraction

Main use case:

- `CreateTripCommand`

It validates that:

- Cargo exists and is pending
- Vehicle exists and is idle
- Driver exists and is available

Then it creates a trip and updates statuses inside one database transaction.

### Infrastructure Layer

Contains:

- EF Core `DbContext`
- PostgreSQL configuration
- Entity configurations via Fluent API
- Repository implementations
- Database seeding
- Transaction implementation

### WebApi Layer

Contains:

- Minimal API endpoints with `MapGroup`
- Swagger configuration
- JWT Bearer security definition
- Global exception handling

---

## Backend API

### Trips

#### Get Active Trips

```http
GET /api/trips/active
```

Returns active trips.

#### Create Trip

```http
POST /api/trips
```

Creates a new trip.

Example request:

```json
{
  "cargoId": "00000000-0000-0000-0000-000000000000",
  "vehicleId": "00000000-0000-0000-0000-000000000000",
  "driverId": "00000000-0000-0000-0000-000000000000",
  "origin": "Warsaw",
  "destination": "Berlin",
  "scheduledStartUtc": "2026-05-14T10:00:00Z",
  "scheduledEndUtc": "2026-05-14T18:00:00Z"
}
```

### Vehicles

#### Check Vehicle Availability

```http
GET /api/vehicles/{id}/availability
```

Checks whether a vehicle is available.

---

## Local Development

### Requirements

- .NET 9 SDK
- Node.js 20+
- Angular CLI 18
- Docker Desktop
- PostgreSQL via Docker

---

## Run PostgreSQL

From the root folder:

```powershell
cd C:\Studies\Pet_projects\LogiFlow
docker compose up -d postgres
```

PostgreSQL connection:

| Property | Value |
|---|---|
| Host | `localhost` |
| Port | `5432` |
| Database | `logiflow` |
| Username | `postgres` |
| Password | `postgres` |

---

## Run Backend Locally

Open this solution in Rider:

```text
Backend/LodiFlowBackend/LodiFlowBackend.sln
```

Run the following project:

```text
LogiFlow.WebApi
```

Or run it from terminal:

```powershell
cd C:\Studies\Pet_projects\LogiFlow\Backend\LodiFlowBackend
dotnet run --project LogiFlow.WebApi\LogiFlow.WebApi.csproj
```

Swagger will be available at the URL printed in the console, for example:

```text
http://localhost:5081/swagger
```

---

## Run Frontend Locally

From the frontend folder:

```powershell
cd C:\Studies\Pet_projects\LogiFlow\Frontend
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

The target port must match the backend HTTP port.

---

## Docker

The project includes Docker configuration for:

- PostgreSQL
- Backend API
- Angular frontend hosted by Nginx

Run from the root folder:

```powershell
cd C:\Studies\Pet_projects\LogiFlow
docker compose up --build
```

Expected URLs:

| Service | URL |
|---|---|
| Frontend | `http://localhost:4200` |
| Backend Swagger | `http://localhost:8080/swagger` |
| PostgreSQL | `localhost:5432` |

> **Note:** The backend Docker image uses official Microsoft .NET images from `mcr.microsoft.com`.
> If Docker cannot pull these images due to network issues, run PostgreSQL through Docker and run the backend locally through Rider.

---

## Frontend Features

### Active Trips

The `TripListComponent` displays active trips in a Material table.

It includes:

- Search input
- RxJS `debounceTime`
- Refresh button
- Loading state
- Error handling

### Create Trip

The `CreateTripComponent` contains a reactive form for creating trips.

It includes:

- Cargo ID
- Vehicle ID
- Driver ID
- Origin
- Destination
- Scheduled start
- Scheduled end
- Async vehicle availability validation

---

## HTTP Interceptors

The frontend includes:

- `AuthInterceptor`
- `GlobalHttpErrorInterceptor`

The auth interceptor attaches a bearer token when one exists in local storage.
