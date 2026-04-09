#!/usr/bin/env python3
# Sets dotnet user-secrets for local (non-Docker) development.
# Run from the repo root: python scripts/server/setup-dev-secrets.py [options]
#
# Database modes (--db-mode):
#   docker  — SQL auth pointing at localhost,1433 (Docker-exposed port).
#             DB_SA_PASSWORD is read from .env; pass --db-password to override.
#   local   — Skips the connection string; appsettings.Development.json
#             Trusted_Connection=True is used (Windows auth, local SQL Server).
#
# Options:
#   --db-mode [docker|local]   Database mode (default: docker)
#   --db-password PASS         SA password for Docker mode.
#                              Falls back to DB_SA_PASSWORD in .env.  [crucial for docker]
#   --smtp-host HOST           SMTP host (default: smtp.gmail.com)
#   --smtp-port PORT           SMTP port (default: 587)
#   --smtp-user USER           SMTP username / Gmail address           [non-crucial]
#   --smtp-pass PASS           SMTP password / App password            [non-crucial]
#   --smtp-from FROM           From address (default: same as --smtp-user)

import argparse
import base64
import secrets
import subprocess
import sys
from pathlib import Path

SERVER_PROJECT = "src/BankApp.Server"
REPO_ROOT = Path(__file__).resolve().parent.parent.parent
ENV_FILE = REPO_ROOT / ".env"

DOCKER_CONN_STR = (
    "Server=localhost,1433;Database=BankAppDb;"
    "User Id=sa;Password={password};TrustServerCertificate=True;"
)

SMTP_PLACEHOLDER_HOST = "smtp.example.com"
SMTP_PLACEHOLDER_USER = "dev@example.com"
SMTP_PLACEHOLDER_PASS = "placeholder"


def set_secret(key: str, value: str) -> None:
    result = subprocess.run(
        ["dotnet", "user-secrets", "set", "--project", SERVER_PROJECT, key, value],
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        print(f"[ERROR] Failed to set {key}: {result.stderr.strip()}", file=sys.stderr)
        sys.exit(1)
    print(f"[OK]   {key:<40} set")


def read_env_value(key: str) -> str | None:
    """Return the value of key from .env, or None if the file or key is absent."""
    if not ENV_FILE.exists():
        return None
    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if line.startswith("#") or "=" not in line:
            continue
        k, _, v = line.partition("=")
        if k.strip() == key:
            return v.strip()
    return None


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Set dotnet user-secrets for local BankApp.Server development.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "examples:\n"
            "  # Docker DB — reads DB_SA_PASSWORD and OTP_SERVER_SECRET from .env\n"
            "  python scripts/server/setup-dev-secrets.py\n\n"
            "  # Docker DB — explicit SA password (skips .env lookup)\n"
            "  python scripts/server/setup-dev-secrets.py --db-password <password>\n\n"
            "  # Local SQL Server (Windows auth, no connection string secret needed)\n"
            "  python scripts/server/setup-dev-secrets.py --db-mode local\n\n"
            "  # With real SMTP credentials\n"
            "  python scripts/server/setup-dev-secrets.py \\\n"
            "      --smtp-user you@gmail.com --smtp-pass <app-password>\n\n"
            "notes:\n"
            "  Jwt:Secret and Otp:ServerSecret are always set.\n"
            "  In docker mode, DB_SA_PASSWORD must be available via --db-password or .env\n"
            "  (run setup-dev-env.py first to generate .env).\n"
            "  SMTP settings are non-crucial — omitting them writes placeholders; email\n"
            "  sends will fail at runtime but everything else keeps working."
        ),
    )
    parser.add_argument(
        "--db-mode",
        choices=["docker", "local"],
        default="docker",
        help="Database mode: 'docker' (SQL auth, localhost:1433) or 'local' (Windows auth). Default: docker.",
    )
    parser.add_argument("--db-password", default=None, metavar="PASS", help="SA password for Docker mode (overrides .env).")
    parser.add_argument("--smtp-host", default=None, metavar="HOST", help="SMTP host (default: smtp.gmail.com).")
    parser.add_argument("--smtp-port", default="587", metavar="PORT", help="SMTP port (default: 587).")
    parser.add_argument("--smtp-user", default=None, metavar="USER", help="SMTP username / Gmail address.")
    parser.add_argument("--smtp-pass", default=None, metavar="PASS", help="SMTP password / App password.")
    parser.add_argument("--smtp-from", default=None, metavar="FROM", help="From address (default: same as --smtp-user).")
    args = parser.parse_args()

    print("\nBankApp.Server — dev secrets setup\n------------------------------------")

    # --- JWT (always generated fresh) ---
    jwt_secret = base64.b64encode(secrets.token_bytes(48)).decode()
    set_secret("Jwt:Secret", jwt_secret)

    # --- Database connection string ---
    docker_mode = args.db_mode == "docker"
    if docker_mode:
        sa_password = args.db_password or read_env_value("DB_SA_PASSWORD")
        if not sa_password:
            print(
                f"[ERROR] Docker mode requires a DB SA password.\n"
                f"        Pass --db-password, or run scripts/server/setup-dev-env.py first to generate .env.",
                file=sys.stderr,
            )
            sys.exit(1)
        if args.db_password:
            print(f"[OK]   DB_SA_PASSWORD from --db-password flag")
        else:
            print(f"[OK]   DB_SA_PASSWORD read from {ENV_FILE.name}")
        set_secret("ConnectionStrings:DefaultConnection", DOCKER_CONN_STR.format(password=sa_password))
    else:
        print("[INFO] db-mode=local — skipping ConnectionStrings secret; appsettings.Development.json will be used.")

    # --- OTP server secret ---
    if docker_mode:
        otp_secret = read_env_value("OTP_SERVER_SECRET")
        if otp_secret:
            print(f"[OK]   OTP_SERVER_SECRET read from {ENV_FILE.name}")
        else:
            print(f"[WARN] OTP_SERVER_SECRET not found in {ENV_FILE.name}. Generating a fresh one.")
            otp_secret = secrets.token_hex(16)
    else:
        otp_secret = secrets.token_hex(16)
        print(f"[OK]   OTP_SERVER_SECRET generated")
    set_secret("Otp:ServerSecret", otp_secret)

    # --- SMTP (non-crucial: warn and use placeholders when not supplied) ---
    smtp_missing = not args.smtp_user or not args.smtp_pass
    if smtp_missing:
        print(
            "\n[WARN] --smtp-user / --smtp-pass not provided. "
            "Placeholder SMTP secrets written — email sends will fail at runtime.\n"
            "       Re-run with the SMTP flags to configure real credentials."
        )

    smtp_host = args.smtp_host or SMTP_PLACEHOLDER_HOST
    smtp_port = args.smtp_port
    smtp_user = args.smtp_user or SMTP_PLACEHOLDER_USER
    smtp_pass = args.smtp_pass or SMTP_PLACEHOLDER_PASS
    smtp_from = args.smtp_from or smtp_user

    set_secret("Email:SmtpHost", smtp_host)
    set_secret("Email:SmtpPort", smtp_port)
    set_secret("Email:SmtpUser", smtp_user)
    set_secret("Email:SmtpPass", smtp_pass)
    set_secret("Email:FromAddress", smtp_from)

    print(
        "\nAll secrets configured.\n"
        "Run 'dotnet user-secrets list --project src/BankApp.Server' to verify.\n"
    )


if __name__ == "__main__":
    main()
