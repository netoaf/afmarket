# Refeitório — school cafeteria system

A two-project ASP.NET Core system for a school cafeteria: students browse the menu and products, keep a cart, reserve weekly meal tickets and manage a balance; staff and admins manage products, categories, the weekly menu and users.

Built by me, alone, as a school project (.NET 8, EF Core, SQL Server). A companion Windows Forms back-office app for teachers, students and grades (written by a classmate) talked to this API through API keys; it is not part of this repository.

## Architecture

```
Browser -> MVC app (Razor, ViewModels only) -> HTTP -> Web API (services, repositories, EF Core) -> SQL Server
```

- `refeitorio/` — ASP.NET Core MVC front end. No database access; it only calls the API.
- `Refeitorio_API/` — Web API: controllers, DTOs, services, repositories, domain models, EF Core migrations.
- `RefeitorioDB.sql` — schema and demo seed data.
- `TECHNICAL_DECISIONS.md` — why the layers are split this way.
- `LIMITATIONS.md` — what is missing and what I would do next.

## Features

- Products with categories, search, vegetarian flag and nutrition info
- Weekly menu (regular and vegetarian) and meal-ticket reservations
- Cart, orders, order history, favourites, balance
- Roles: Student, Staff, Admin (admin and staff screens for products, categories, menu, users)

## Security notes

- Passwords are hashed with ASP.NET Core Identity `PasswordHasher`; no plain-text passwords in code or seed data.
- Public registration always creates a Student; the client cannot choose a role.
- Connection strings contain no credentials (Windows authentication).
- Known gap: the API itself does not yet require a token (see `LIMITATIONS.md`).

## Run it

Requires the .NET 8 SDK and SQL Server (Express or LocalDB). See `SETUP.md`.

```powershell
sqlcmd -S ".\SQLEXPRESS" -f 65001 -i RefeitorioDB.sql
dotnet run --project Refeitorio_API/WebApplication1   # https://localhost:7091
dotnet run --project refeitorio/refeitorio            # https://localhost:7073
```

Demo accounts (local only): `admin`, `student`, `func`, all with password `Demo-2026!`.

These accounts exist only in the demo seed script. Delete them before any real deployment.

## Honest scope

School project. No automated tests yet; product photos are not included in this repository. A production version would add API authentication, tests, and a CI pipeline.
