# TicoAutos - Vehicle Marketplace System 

##  Course Information
* **Course:** Web Environment Programming II (ISW-711)
* **Institution:** Universidad Técnica Nacional (UTN)
* **Instructor:** [Bladimir Arroyo](https://github.com/barroyo)
* **Student:** Alvaro Victor Zamora

---

## Introduction
Project for Software Engineering. TicoAutos is a professional-grade web platform for vehicle commerce, built with a decoupled architecture focusing on scalability and clean code standards.

##  Documentation
Official project documentation and requirements can be found in the `/docs` folder:
- [ Project Documentation- TicoAutos](./docs/ProyectoTicoAutos.pdf)

##  Architectural Vision: Clean Architecture
This project strictly follows the **Onion Architecture** pattern to decouple business logic from external frameworks:

* **TicoAutos.Domain:** The core. Contains Entities, Enums, and Repository Interfaces. No dependencies.
* **TicoAutos.Application:** Use cases, DTOs, Mapping profiles, and Service Interfaces.
* **TicoAutos.Infrastructure:** Data access (EF Core), Unit of Work, and Security (JWT).
* **TicoAutos.WebApi:** Controllers, Middlewares, and API Configuration.
---
## Project Structure (Solution Tree)

```
TicoAutos/
├── src/
│   ├── TicoAutos.Domain/                # Core Layer: Entities & Contracts
│   │   ├── Common/
│   │   │   └── BaseEntity.cs            # Shared logic (Id, CreatedAt)
│   │   ├── Entities/
│   │   │   ├── Vehicle.cs               # Inherits from BaseEntity
│   │   │   ├── Question.cs
│   │   │   └── Answer.cs
│   │   └── Interfaces/
│   │       ├── IUnitOfWork.cs           # Transaction orchestrator
│   │       ├── IVehicleRepository.cs
│   │       └── IIdentityService.cs
│   │
│   ├── TicoAutos.Application/           # Logic Layer: DTOs & Validation
│   │   ├── DTOs/
│   │   │   ├── Identity/                # Auth Requests/Responses
│   │   │   └── Vehicles/                # Vehicle CRUD DTOs
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs        # AutoMapper configurations
│   │   ├── Validators/
│   │   │   └── Vehicles/                # FluentValidation rules
│   │   └── Extensions/
│   │       └── DependencyInjection.cs   # Application service registration
│   │
│   ├── TicoAutos.Infrastructure/        # External Layer: DB & Identity
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Configurations/          # Fluent API table mappings
│   │   ├── Repositories/
│   │   │   ├── UnitOfWork.cs            # Pattern implementation
│   │   │   └── VehicleRepository.cs
│   │   ├── Identity/
│   │   │   └── IdentityService.cs       # JWT & Auth implementation
│   │   ├── Migrations/                  # EF Core versioning
│   │   └── Extensions/
│   │       └── DependencyInjection.cs   # Infrastructure service registration
│   │
│   └── TicoAutos.WebApi/                # Interface Layer: API Entry Point
│       ├── Controllers/
│       │   └── AuthController.cs        # Auth endpoints
│       ├── Program.cs                   # App & DI call
│       └── appsettings.json             # DB Connection & JWT Keys
```
---

##  Tech Stack & Patterns
- **Language/Framework:** C# / .NET 9
- **Database:** SQL Server + Entity Framework Core
- **Patterns:** Unit of Work, Repository, DTOs, Dependency Injection.
- **Security:** JWT (JSON Web Tokens).
- **Frontend:** Angular (Standalone).
---
##  Development Workflow
We use **GitHub Issues** to track progress and **GitFlow** for branching.
- `main`: Production-ready code (Final Delivery).
- `develop`: Integration branch.
- `feat/`: Feature-specific branches.
---
