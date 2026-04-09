# BankApp.Server

ASP.NET Core Web API backend for BankApp.

## Running with Docker (recommended)

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### First-time setup

```bash
# Generate .env with auto-created secrets (SMTP optional — see flags below)
python scripts/server/setup-dev-env.py --force

docker compose up --build
```

The API is available at **http://localhost:5024** once the server prints `Application started`.

`--build` is only needed on first run or after code changes.

### `setup-dev-env.py` — generates `.env` for Docker Compose

All cryptographic secrets (`DB_SA_PASSWORD`, `JWT_SECRET`, `OTP_SERVER_SECRET`) are
auto-generated. SMTP is optional — omitting it writes placeholders and logs a warning;
everything except email delivery keeps working.

```
usage: setup-dev-env.py [--force]
                        [--smtp-host HOST] [--smtp-port PORT]
                        [--smtp-user USER] [--smtp-pass PASS]
                        [--smtp-from FROM]

  --force           Overwrite an existing .env without erroring out.
  --smtp-host HOST  SMTP hostname          (default: smtp.gmail.com)
  --smtp-port PORT  SMTP port              (default: 587)
  --smtp-user USER  SMTP username / Gmail address      [non-crucial]
  --smtp-pass PASS  SMTP password / App password       [non-crucial]
  --smtp-from FROM  Sender address         (default: same as --smtp-user)
```

Examples:
```bash
# Minimal — SMTP uses placeholders, warns at startup
python scripts/server/setup-dev-env.py --force

# With real email credentials
python scripts/server/setup-dev-env.py --force \
    --smtp-user you@gmail.com --smtp-pass <app-password>
```

Run `python scripts/server/setup-dev-env.py --help` for full usage.

### Connecting to the database from your IDE

The SQL Server container exposes port **1433** on localhost. Use these credentials:

| Field    | Value                                             |
|----------|---------------------------------------------------|
| Host     | `localhost`                                       |
| Port     | `1433`                                            |
| Auth     | SQL Server / User & Password                      |
| User     | `sa`                                              |
| Password | value of `DB_SA_PASSWORD` in `.env` (see below)   |
| Database | `BankAppDb`                                       |

To find the password, open `.env` in the repo root, or run one of:
```bash
grep DB_SA_PASSWORD .env   # grep
rg DB_SA_PASSWORD .env     # ripgrep
```

> **If the connection is refused or the password is wrong**, the volume may have been
> initialised with a different password. Run `docker compose down -v && docker compose up --build`
> to reset it. This wipes all data and re-runs the schema and seed scripts.

### Useful commands

```bash
docker compose up -d          # start in background
docker compose logs -f server # tail server logs
docker compose down           # stop (data volume preserved)
docker compose down -v        # stop and delete database volume
```

## Running without Docker

### Prerequisites

- .NET 10 SDK
- SQL Server (local instance **or** the Docker DB started separately with `docker compose up db db-init`)

### First-time setup

```bash
# Configure user-secrets (Docker DB mode by default — reads .env for SA password)
python scripts/server/setup-dev-secrets.py

dotnet run --project src/BankApp.Server
```

### `setup-dev-secrets.py` — sets dotnet user-secrets for local development

```
usage: setup-dev-secrets.py [--db-mode {docker,local}]
                            [--db-password PASS]
                            [--smtp-host HOST] [--smtp-port PORT]
                            [--smtp-user USER] [--smtp-pass PASS]
                            [--smtp-from FROM]

  --db-mode {docker,local}
                    docker — SQL auth to localhost,1433 (Docker-exposed port).
                             Reads DB_SA_PASSWORD from .env automatically.
                    local  — Skips connection string; appsettings.Development.json
                             Trusted_Connection=True is used (Windows auth).
                    Default: docker
  --db-password PASS  SA password for Docker mode. Overrides .env lookup.  [crucial in docker mode]
  --smtp-host HOST  SMTP hostname          (default: smtp.gmail.com)
  --smtp-port PORT  SMTP port              (default: 587)
  --smtp-user USER  SMTP username / Gmail address      [non-crucial]
  --smtp-pass PASS  SMTP password / App password       [non-crucial]
  --smtp-from FROM  Sender address         (default: same as --smtp-user)
```

Examples:
```bash
# Docker DB — reads credentials from .env (run setup-dev-env.py first)
python scripts/server/setup-dev-secrets.py

# Docker DB — explicit SA password
python scripts/server/setup-dev-secrets.py --db-password <password>

# Local SQL Server (Windows auth)
python scripts/server/setup-dev-secrets.py --db-mode local

# With real email credentials
python scripts/server/setup-dev-secrets.py \
    --smtp-user you@gmail.com --smtp-pass <app-password>
```

Run `python scripts/server/setup-dev-secrets.py --help` for full usage.

## Configuration reference

| Key                                   | Docker / .env var             | Description                                  |
|---------------------------------------|-------------------------------|----------------------------------------------|
| `ConnectionStrings:DefaultConnection` | `DB_SA_PASSWORD` (composed)   | ADO.NET connection string                    |
| `Jwt:Secret`                          | `JWT_SECRET`                  | JWT signing key (min 32 chars)               |
| `Otp:ServerSecret`                    | `OTP_SERVER_SECRET`           | Server-side HMAC seed for TOTP generation    |
| `Email:SmtpHost`                      | `EMAIL_SMTP_HOST`             | SMTP server hostname                         |
| `Email:SmtpPort`                      | `EMAIL_SMTP_PORT`             | SMTP port (587 for STARTTLS)                 |
| `Email:SmtpUser`                      | `EMAIL_SMTP_USER`             | SMTP username                                |
| `Email:SmtpPass`                      | `EMAIL_SMTP_PASS`             | SMTP password / app password                 |
| `Email:FromAddress`                   | `EMAIL_FROM_ADDRESS`          | Sender address for outgoing emails           |

## Production

Set the same environment variables on your host. Use `__` instead of `:` for nested keys (e.g. `Jwt__Secret`). Set `ASPNETCORE_ENVIRONMENT=Production` to disable Swagger and developer error pages.
