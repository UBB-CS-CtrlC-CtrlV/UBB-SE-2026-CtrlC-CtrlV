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

### 1. Server

```bash
# Generate .env with auto-created secrets (re-run with --force to regenerate)
python scripts/server/setup-dev-env.py --force

docker compose up --build
```

API available at **http://localhost:5024**. The database schema and seed data are applied automatically on first start.

See [`src/BankApp.Server`](src/BankApp.Server/README.md) for the full server setup guide, script flag reference, and database connection instructions.

### 2. Client

```bash
# Write appsettings.Local.json with your Google OAuth credentials
python scripts/client/setup-dev-config.py \
    --client-id <your-client-id> \
    --client-secret <your-client-secret>
```

Obtain credentials: Google Cloud Console → APIs & Services → Credentials → OAuth 2.0 Client IDs.

Open `BankApp.slnx`, set `BankApp.Client` as the startup project, select the `x64` platform, and press F5. The server must be running first.

See [`src/BankApp.Client`](src/BankApp.Client/README.md) for the full client setup guide.

## Prerequisites

- Windows 10/11 (client)
- .NET SDK 10.x
- Docker Desktop (server)
- Python 3.6+ (setup scripts)
