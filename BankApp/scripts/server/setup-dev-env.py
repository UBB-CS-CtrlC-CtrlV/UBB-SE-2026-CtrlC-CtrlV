#!/usr/bin/env python3
# Generates .env for Docker Compose. All secrets are auto-generated.
# SMTP settings are optional — omitting them writes placeholders and logs a warning.
#
# Usage:
#   python scripts/server/setup-dev-env.py [options]
#
# Options:
#   --force                    Overwrite an existing .env without prompting.
#   --smtp-host HOST           SMTP host          (default: smtp.gmail.com)
#   --smtp-port PORT           SMTP port          (default: 587)
#   --smtp-user USER           SMTP username / Gmail address  [non-crucial]
#   --smtp-pass PASS           SMTP password / App password   [non-crucial]
#   --smtp-from FROM           From address       (default: same as --smtp-user)
#
# All other values (DB_SA_PASSWORD, JWT_SECRET, OTP_SERVER_SECRET) are always
# generated fresh; there is no flag to supply them manually.

import argparse
import base64
import secrets
import string
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
ENV_FILE = REPO_ROOT / ".env"

SMTP_PLACEHOLDER_HOST = "smtp.example.com"
SMTP_PLACEHOLDER_USER = "dev@example.com"
SMTP_PLACEHOLDER_PASS = "placeholder"


def gen_db_password(length: int = 20) -> str:
    """Random password satisfying SQL Server complexity rules."""
    specials = "!@#$%^&*"
    pool = string.ascii_letters + string.digits + specials
    while True:
        pwd = "".join(secrets.choice(pool) for _ in range(length))
        if (
            any(c.isupper() for c in pwd)
            and any(c.islower() for c in pwd)
            and any(c.isdigit() for c in pwd)
            and any(c in specials for c in pwd)
        ):
            return pwd


def gen_jwt_secret() -> str:
    """64-character base64 string from 48 random bytes."""
    return base64.b64encode(secrets.token_bytes(48)).decode()


def gen_otp_secret() -> str:
    """32-character hex string used as the server-side OTP HMAC seed."""
    return secrets.token_hex(16)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate .env for BankApp Docker Compose.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "examples:\n"
            "  # Minimal — auto-generates all secrets, SMTP uses placeholders (warns)\n"
            "  python scripts/server/setup-dev-env.py --force\n\n"
            "  # With real SMTP credentials\n"
            "  python scripts/server/setup-dev-env.py --force \\\n"
            "      --smtp-user you@gmail.com --smtp-pass <app-password>\n\n"
            "notes:\n"
            "  DB_SA_PASSWORD, JWT_SECRET, and OTP_SERVER_SECRET are always auto-generated.\n"
            "  SMTP settings are non-crucial — omitting them writes placeholders; email\n"
            "  sends will fail at runtime but everything else keeps working.\n"
            "  Run with --force to regenerate an existing .env."
        ),
    )
    parser.add_argument("--force", action="store_true", help="Overwrite existing .env.")
    parser.add_argument("--smtp-host", default=None, metavar="HOST", help="SMTP host (default: smtp.gmail.com).")
    parser.add_argument("--smtp-port", default="587", metavar="PORT", help="SMTP port (default: 587).")
    parser.add_argument("--smtp-user", default=None, metavar="USER", help="SMTP username / Gmail address.")
    parser.add_argument("--smtp-pass", default=None, metavar="PASS", help="SMTP password / App password.")
    parser.add_argument("--smtp-from", default=None, metavar="FROM", help="From address (default: same as --smtp-user).")
    args = parser.parse_args()

    print("\nBankApp.Server — Docker .env setup\n------------------------------------")

    if ENV_FILE.exists() and not args.force:
        print(f"[ERROR] {ENV_FILE} already exists. Pass --force to overwrite.", file=sys.stderr)
        sys.exit(1)

    db_password = gen_db_password()
    jwt_secret = gen_jwt_secret()
    otp_secret = gen_otp_secret()
    print(f"[OK]   {'DB_SA_PASSWORD':<25} generated ({len(db_password)} chars)")
    print(f"[OK]   {'JWT_SECRET':<25} generated ({len(jwt_secret)} chars)")
    print(f"[OK]   {'OTP_SERVER_SECRET':<25} generated ({len(otp_secret)} chars)")

    # SMTP — non-crucial: warn and use placeholders when not supplied.
    smtp_missing = not args.smtp_user or not args.smtp_pass
    if smtp_missing:
        print(
            "\n[WARN] --smtp-user / --smtp-pass not provided. "
            "Placeholder SMTP values written — email sends will fail at runtime.\n"
            "       Re-run with --force and the SMTP flags to fix this later."
        )

    smtp_host = args.smtp_host or SMTP_PLACEHOLDER_HOST
    smtp_port = args.smtp_port
    smtp_user = args.smtp_user or SMTP_PLACEHOLDER_USER
    smtp_pass = args.smtp_pass or SMTP_PLACEHOLDER_PASS
    smtp_from = args.smtp_from or smtp_user

    if not smtp_missing:
        print(f"[OK]   {'EMAIL_SMTP_HOST':<25} {smtp_host}")
        print(f"[OK]   {'EMAIL_SMTP_USER':<25} {smtp_user}")

    ENV_FILE.write_text(
        f"# Generated by scripts/server/setup-dev-env.py — do not commit.\n\n"
        f"DB_SA_PASSWORD={db_password}\n"
        f"JWT_SECRET={jwt_secret}\n"
        f"OTP_SERVER_SECRET={otp_secret}\n\n"
        f"EMAIL_SMTP_HOST={smtp_host}\n"
        f"EMAIL_SMTP_PORT={smtp_port}\n"
        f"EMAIL_SMTP_USER={smtp_user}\n"
        f"EMAIL_SMTP_PASS={smtp_pass}\n"
        f"EMAIL_FROM_ADDRESS={smtp_from}\n",
        encoding="utf-8",
    )

    print(f"\n[OK]   Written to {ENV_FILE}\n\nNext steps:\n  docker compose up --build\n")


if __name__ == "__main__":
    main()
