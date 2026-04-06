# BankApp

A personal banking application with a WinUI 3 desktop client and an ASP.NET Core REST API backend.

## Tech stack

| Layer | Technology |
|---|---|
| Desktop client | WinUI 3 / Windows App SDK (unpackaged) |
| API server | ASP.NET Core 10, ADO.NET, SQL Server |
| Auth | JWT, Google OAuth 2.0, TOTP 2FA |
| Database | SQL Server 2025 (Docker) |
| Containerisation | Docker Compose |

## Structure

```
src/
  BankApp.Contracts/   shared DTOs, entities, and enums
  BankApp.Server/      ASP.NET Core API
  BankApp.Client/      WinUI 3 desktop client
tests/
  BankApp.Contracts.Tests/
  BankApp.Server.Tests/
  BankApp.Client.Tests/
scripts/
  db/                  schema.sql — applied automatically on first docker compose up
  server/              setup-dev-env.py, setup-dev-secrets.py
  client/              setup-dev-config.py
```

## Quick start

### Server (Docker — recommended)

```bash
python scripts/server/setup-dev-env.py   # generates .env
docker compose up --build
```

API available at **http://localhost:5024**. The database schema is applied automatically on first start.

See [`src/BankApp.Server`](src/BankApp.Server/README.md) for the full server setup guide.

### Client

```bash
python scripts/client/setup-dev-config.py   # writes appsettings.Local.json with OAuth credentials
```

Open `BankApp.slnx`, set `BankApp.Client` as the startup project, select the `x64` platform, and press F5. The server must be running first.

See [`src/BankApp.Client`](src/BankApp.Client/README.md) for the full client setup guide.

## Prerequisites

- Windows 10/11 (client)
- .NET SDK 10.x
- Docker Desktop (server)
- Python 3.6+ (setup scripts)
