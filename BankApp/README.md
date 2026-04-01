# BankApp

BankApp has two runnable projects:

- `src/BankApp.Server` is the ASP.NET Core API.
- `src/BankApp.Client` is the WinUI 3 desktop client.

For local development, run the server first and then the client. The client is configured to call the server at `http://localhost:5024`.

## Prerequisites

- Windows
- .NET SDK 10.x
- Local SQL Server instance reachable on `localhost`
- Windows Developer Mode enabled

### Visual Studio

Install these workloads:

- `ASP.NET and web development`
- `WinUI application development`

## Open The Solution

Open `BankApp.slnx` from the repository root.

## Database Setup

The SQL schema now lives in [src/BankApp.Infrastructure/Database/BankAppDatabaseSchema.sql](/C:/Users/arby/Workspace/github.com/UBB-CS-CtrlC-CtrlV/UBB-SE-2026-CtrlC-CtrlV/BankApp/src/BankApp.Infrastructure/Database/BankAppDatabaseSchema.sql).

1. Create a database named `BankAppDb` in your local SQL Server instance.
2. Execute the schema script against `BankAppDb`.
3. Make sure the connection string in [src/BankApp.Server/appsettings.json](/C:/Users/arby/Workspace/github.com/UBB-CS-CtrlC-CtrlV/UBB-SE-2026-CtrlC-CtrlV/BankApp/src/BankApp.Server/appsettings.json) matches your local setup.

Default connection string:

```text
Server=localhost;Database=BankAppDb;Trusted_Connection=True;TrustServerCertificate=True;
```

## Run The App

1. Start `BankApp.Server`.
2. Confirm the API is running at `http://localhost:5024/swagger`.
3. Start `BankApp.Client`.

The server launch URL is configured in [src/BankApp.Server/Properties/launchSettings.json](/C:/Users/arby/Workspace/github.com/UBB-CS-CtrlC-CtrlV/UBB-SE-2026-CtrlC-CtrlV/BankApp/src/BankApp.Server/Properties/launchSettings.json).

## Visual Studio Setup

To start both projects together:

1. Open the solution.
2. Right-click the solution.
3. Choose `Configure Startup Projects`.
4. Select `Multiple startup projects`.
5. Set `BankApp.Server` to `Start`.
6. Set `BankApp.Client` to `Start`.
7. Keep `BankApp.Server` first.

If your Visual Studio version supports shared launch profiles, save the profile so the team can reuse it.

## Rider Setup

For Rider, create:

- a run configuration for `BankApp.Server`
- a run configuration for `BankApp.Client`
- a `Compound` configuration that starts both

Use the `http` launch profile for the server from [src/BankApp.Server/Properties/launchSettings.json](/C:/Users/arby/Workspace/github.com/UBB-CS-CtrlC-CtrlV/UBB-SE-2026-CtrlC-CtrlV/BankApp/src/BankApp.Server/Properties/launchSettings.json).

