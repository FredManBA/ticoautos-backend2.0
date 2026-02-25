# TicoAutos - Vehicle Marketplace System 

##  Course Information
* **Course:** Web Environment Programming II (ISW-711)
* **Institution:** Universidad Técnica Nacional (UTN)
* **Instructor:** [Bladimir Arroyo](https://github.com/barroyo)
* **Student:** Alvaro Victor Zamora

---

##  Introduction
Project for Software Engineering. TicoAutos is a professional-grade web platform for vehicle commerce, built with a decoupled architecture focusing on scalability and clean code standards.

##  Documentation
Official project documentation and requirements can be found in the `/docs` folder:
- [ Primer Proyecto - TicoAutos](./docs/Primer%20Proyecto%20-%20TicoAutos.pdf)

---

##  Architectural Vision: Clean Architecture
This project strictly follows the **Onion Architecture** pattern to decouple business logic from external frameworks:

* **TicoAutos.Domain:** The core. Contains Entities, Enums, and Repository Interfaces. No dependencies.
* **TicoAutos.Application:** Use cases, DTOs, Mapping profiles, and Service Interfaces.
* **TicoAutos.Infrastructure:** Data access (EF Core), Persistence, Unit of Work, and Security.
* **TicoAutos.WebApi:** Controllers, Middlewares, and API Configuration.



---

##  Tech Stack & Patterns
- **Language/Framework:** C# / .NET 8
- **Database:** SQL Server + Entity Framework Core
- **Patterns:** Unit of Work, Repository, DTOs, Dependency Injection, Extension Methods.
- **Security:** JWT (JSON Web Tokens) - *In Progress*.
- **Frontend:** Angular (Standalone) - *In Progress*.

---

##  Development Workflow
We use **GitHub Issues** to track progress and **GitFlow** for branching.
- `main`: Production-ready code (Final Delivery).
- `develop`: Integration branch.
- `feat/`: Feature-specific branches.

---

##  How to Run the Database
1. Update the connection string in `appsettings.json` within the **WebApi** project.
2. Open a terminal in the solution root and run:
   ```bash
   dotnet ef database update --project TicoAutos.Infrastructure --startup-project TicoAutos.WebApi