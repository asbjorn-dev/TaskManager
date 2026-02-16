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
- Docker Compose (multi-container orchestration)
- Kubernetes (container orchestration)
- xUnit + Moq (unit tests)
- Swagger/OpenAPI

## Patterns

- **Repository Pattern** - abstraktion over data access
- **Service Layer** - business logik separeret fra controllers
- **DTO Pattern** - Data Transfer Objects separerer API-kontrakt fra interne modeller
- **Cache-Aside Pattern** - Redis cache med invalidering på write-operationer
- **Event-Driven Architecture** - asynkron kommunikation via RabbitMQ
- **Database per Service** - hver microservice ejer sin database
- **Monolith-first strategy** - start med en simpel monolit og splitt til microservices når kompleksitet og skala kræver det

## Kom igang

### Forudsætninger

- .NET 10 SDK
- Docker Desktop

## Docker Hub
Alle service images er tilgængelige på Docker Hub med semantisk versionering:
| Image | Stabil version |
|-----------|-------------|
| asbjorndev/taskmanagement-api | 1.0.0 |
| asbjorndev/taskmanagement-identity | 1.0.0 |
| asbjorndev/taskmanagement-notifications | 1.0.0 |

### Option 1: Docker Compose

Start hele stacken med én kommando:

```bash
docker compose up
```

Dette starter alle services og infrastructure automatisk:

| Container | Beskrivelse | URL |
|-----------|-------------|-----|
| **Core API** | REST API | `http://localhost:5165/swagger` |
| **Identity Service** | Auth API | `http://localhost:5215/swagger` |
| **Notification Service** | Worker (ingen port) | Logs i terminal |
| **RabbitMQ** | Message broker | `http://localhost:15672` (guest/guest) |
| **Redis** | Cache | - |
| **RedisInsight** | Redis UI | `http://localhost:5540` |

Stop med:

```bash
docker compose down
```

### Option 2: Kubernetes

Forudsætter Kubernetes aktiveret i Docker Desktop.

```bash
kubectl apply -f k8s/
```

| Service | URL |
|---------|-----|
| **Core API** | `http://localhost:30165/swagger` |
| **Identity Service** | `http://localhost:30215/swagger` |
| **RabbitMQ UI** | `http://localhost:30672` (guest/guest) |
| **RedisInsight** | `http://localhost:30540` |

Stop med:

```bash
kubectl delete -f k8s/
```

### Option 3: Lokal udvikling (uden Docker)

Start infrastructure manuelt:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management
docker run -d --name redis -p 6379:6379 redis:latest
docker run -d --name redisinsight -p 5540:5540 redis/redisinsight:latest
```

Start services i separate terminaler:

```bash
cd TaskManagement.Identity && dotnet run
cd TaskManagement.Api && dotnet run
cd TaskManagement.Notifications && dotnet run
```

### Brug API'et

1. Gå til Identity Swagger og opret en bruger via `POST /api/auth/register`
2. Login via `POST /api/auth/login` og kopier JWT token
3. Gå til Core API Swagger og klik "Authorize" - indsæt token
4. Nu kan du oprette Teams, Projects og Tasks
5. Admins kan slette Teams, projects og Tasks (kræver admin token)


### Kør tests
```bash
dotnet test
```

## Roadmap
- [x] Monolit med REST API, Repository Pattern og Unit Tests
- [x] Identity Service (JWT Auth)
- [x] RabbitMQ + Notification Service
- [x] Redis Caching
- [x] Docker Compose
- [x] Kubernetes deployment
- [ ] Arkitektur-diagrammer (draw.io)
- [ ] Forbedre test coverage
- [ ] Videreudvikle Notification Service (email/Slack)
