# BankApp.Client

WinUI 3 desktop client for BankApp. Runs unpackaged (`WindowsPackageType=None`) — no MSIX required.

The server must be running before launching the client. See [`src/BankApp.Server`](../BankApp.Server/README.md).

## Setup

### `setup-dev-config.py` — writes `appsettings.Local.json`

Both flags are required. The app cannot perform Google OAuth login without them.

```
usage: setup-dev-config.py --client-id ID --client-secret SECRET

  --client-id ID        Google OAuth client ID      [required]
  --client-secret SECRET  Google OAuth client secret  [required]
```

Example:
```bash
python scripts/client/setup-dev-config.py \
    --client-id 123456.apps.googleusercontent.com \
    --client-secret GOCSPX-abc123
```

Creates `src/BankApp.Client/appsettings.Local.json`. Gitignored, never committed. Re-running overwrites it safely.

Obtain credentials: Google Cloud Console → APIs & Services → Credentials → OAuth 2.0 Client IDs.

Run `python scripts/client/setup-dev-config.py --help` for full usage.

## Configuration

| File | Committed | Purpose |
|---|---|---|
| `appsettings.json` | Yes | Safe defaults and placeholders |
| `appsettings.Local.json` | No | Local secret overrides |

Load order: `appsettings.json` → `appsettings.Local.json` → environment variables (`__` as key separator, e.g. `OAuth__Google__ClientId`).

| Key | Default | Secret |
|---|---|---|
| `ApiBaseUrl` | `http://localhost:5024` | No |
| `OAuth:Google:Authority` | `https://accounts.google.com` | No |
| `OAuth:Google:RedirectUri` | `http://127.0.0.1:7890/` | No |
| `OAuth:Google:ClientId` | — | Yes |
| `OAuth:Google:ClientSecret` | — | Yes |

## Running

1. Set `BankApp.Client` as the startup project.
2. Select the `x64` platform.
3. Press F5.

**Visual Studio:** Configure Startup Projects → Multiple startup projects → set both Server and Client to Start (Server first).

**Rider:** Create a Compound run configuration with `BankApp.Server` and `BankApp.Client`.

## Architecture

MVVM pattern throughout.

- **Views** — XAML pages, no business logic. Bind to their ViewModel and forward user actions.
- **ViewModels** — coordinate API calls and hold UI state as `ObservableState<TState>` (wraps state enums).
- **ApiClient** — single HTTP boundary, registered as a singleton to avoid socket exhaustion.
- **AppNavigationService** — wraps the WinUI `Frame`, the only place that triggers page navigation.

DI is set up manually in `App.xaml.cs` using `Microsoft.Extensions.DependencyInjection` (no generic host). The container is available via `App.Services` but accessed only at the composition root in `OnLaunched`. All other classes use constructor injection.
