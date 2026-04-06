#!/usr/bin/env python3
# Sets dotnet user-secrets for local (non-Docker) development.
# Run from the repo root: python scripts/server/setup-dev-secrets.py

import base64
import getpass
import secrets
import subprocess
import sys

SERVER_PROJECT = "src/BankApp.Server"


def set_secret(key: str, value: str) -> None:
    result = subprocess.run(
        ["dotnet", "user-secrets", "set", "--project", SERVER_PROJECT, key, value],
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        print(f"[ERROR] Failed to set {key}: {result.stderr.strip()}", file=sys.stderr)
        sys.exit(1)
    print(f"[OK] {key:<25} set")


def main() -> None:
    print()
    print("BankApp.Server — dev secrets setup")
    print("------------------------------------")

    jwt_secret = base64.b64encode(secrets.token_bytes(48)).decode()
    set_secret("Jwt:Secret", jwt_secret)

    print()
    print("SMTP credentials must be real Gmail values and cannot be generated.")
    print("Use a Gmail App Password (Google Account > Security > 2-Step Verification > App passwords)")
    print()

    smtp_user = input("    Email:SmtpUser    (Gmail address): ").strip()
    smtp_pass = getpass.getpass("    Email:SmtpPass    (App password, input hidden): ")
    smtp_from = input("    Email:FromAddress (press Enter to use same as SmtpUser): ").strip()

    if not smtp_from:
        smtp_from = smtp_user

    set_secret("Email:SmtpUser", smtp_user)
    set_secret("Email:SmtpPass", smtp_pass)
    set_secret("Email:FromAddress", smtp_from)

    print()
    print("All secrets configured. Run 'dotnet user-secrets list --project src/BankApp.Server' to verify.")
    print()


if __name__ == "__main__":
    main()
