# TicoAutos - Vehicle Marketplace System 

##  Course Information
* **Course:** Web Environment Programming II (ISW-711)
* **Institution:** Universidad Técnica Nacional (UTN)
* **Instructor:** [Bladimir Arroyo](https://github.com/barroyo)
* **Student:** Alvaro Victor Zamora

---

## Introduction
Project for Software Engineering. TicoAutos is a professional-grade web platform for vehicle commerce, built with a decoupled architecture focusing on scalability and clean code standards.

##  Architectural Vision: Clean Architecture
This project strictly follows the **Onion Architecture** pattern to decouple business logic from external frameworks:

* **TicoAutos.Domain:** The core. Contains Entities, Enums, and Repository Interfaces. No dependencies.
* **TicoAutos.Application:** Use cases, DTOs, Mapping profiles, and Service Interfaces.
* **TicoAutos.Infrastructure:** Data access (EF Core), Unit of Work, and Security (JWT).
* **TicoAutos.WebApi:** Controllers, Middlewares, and API Configuration.

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
