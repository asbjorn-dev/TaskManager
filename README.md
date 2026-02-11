# Task Management Platform

> **OBS:** Aktiv udvikling sker på [`feature/microservices`](../../tree/feature/microservices) branchen.
> `main` indeholder den oprindelige monolit-version.

Jira-inspireret task management API med relationel datamodel (Teams, Projects, Tasks, many-to-many tagging). Startede som monolit, migreret til microservices arkitektur.

## Arkitektur

| Service | Beskrivelse | Port |
|---------|-------------|------|
| **Core API** | REST API til Teams, Projects og Tasks | 5165 |
| **Identity Service** | JWT authentication og bruger-registrering | 5215 |
| **Notification Service** | Worker service der lytter på RabbitMQ events | - |
| **Shared** | Class library med delte event-modeller | - |

## Teknologier

- ASP.NET Core 10 / EF Core
- SQLite (database per service)
- JWT Authentication med rollebaseret authorization
- RabbitMQ (event-driven kommunikation)
- Redis (distributed caching)
- xUnit + Moq (unit tests)
- Swagger/OpenAPI

## Patterns

- **Repository Pattern** - abstraktion over data access
- **Service Layer** - business logik separeret fra controllers
- **Cache-Aside Pattern** - Redis cache med invalidering på write-operationer
- **Event-Driven Architecture** - asynkron kommunikation via RabbitMQ
- **Database per Service** - hver microservice ejer sin database

## Kom igang

### Forudsætninger

- .NET 10 SDK
- Docker (til RabbitMQ og Redis)

### 1. Start infrastructure

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management
docker run -d --name redis -p 6379:6379 redis:latest
```

## Start services

### Terminal 1 - Identity Service
```bash
cd TaskManagement.Identity
dotnet run
```

### Terminal 2 - Core API
```bash
cd TaskManagement.Api
dotnet run
```

### Terminal 3 - Notification Service
```bash
cd TaskManagement.Notifications
dotnet run
```

### Brug API'et
Gå til http://localhost:5215/swagger (Identity) og opret en bruger via /api/auth/register
Login via /api/auth/login og kopier JWT token

Gå til http://localhost:5165/swagger (Core API) og klik "Authorize" - indsæt token.

Nu kan du oprette Teams, Projects og Tasks


### Kør tests
```bash
dotnet test
```

## Roadmap
- Monolit med REST API, Repository Pattern og Unit Tests ✅
- Identity Service (JWT Auth) ✅
- RabbitMQ + Notification Service ✅
- Redis Caching ✅
- Docker Compose
- Kubernetes deployment
- Arkitektur-diagrammer (draw.io)
- Forbedre test coverage
