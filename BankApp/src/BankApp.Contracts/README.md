# BankApp.Contracts

Shared library referenced by both `BankApp.Server` and `BankApp.Client`. Contains only types — no logic, no dependencies.

## Contents

| Folder | Purpose |
|---|---|
| `DTOs/` | Request and response objects for API endpoints |
| `Entities/` | Plain C# classes that mirror database tables |
| `Enums/` | Domain enums shared across the boundary |
| `Extensions/` | Extension methods on shared types |

## What belongs here

- Types that cross the client–server boundary (DTOs, shared enums, entities used in API responses).
- Extension methods on those types.

## What does not belong here

- UI state enums (e.g. `LoginState`) — those live in `BankApp.Client.Enums`.
- Business logic, services, or anything with dependencies.
- Server-internal types that the client never sees.
