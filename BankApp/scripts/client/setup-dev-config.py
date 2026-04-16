#!/usr/bin/env python3
# Creates src/BankApp.Desktop/appsettings.Local.json with Google OAuth credentials.
# Run from the repo root: python scripts/client/setup-dev-config.py [options]
# Re-running is safe — the file is fully overwritten each time.
#
# Options:
#   --client-id ID         Google OAuth client ID      [required]
#   --client-secret SECRET Google OAuth client secret  [required]
#
# Obtain credentials: Google Cloud Console > APIs & Services > Credentials > OAuth 2.0 Client IDs

import argparse
import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
OUTPUT_PATH = REPO_ROOT / "src" / "BankApp.Desktop" / "appsettings.Local.json"


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Write appsettings.Local.json with Google OAuth credentials for the BankApp client.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "examples:\n"
            "  python scripts/client/setup-dev-config.py \\\n"
            "      --client-id 123456.apps.googleusercontent.com \\\n"
            "      --client-secret GOCSPX-abc123\n\n"
            "notes:\n"
            "  Both flags are required — the app cannot perform Google OAuth login without them.\n"
            "  The output file (appsettings.Local.json) is gitignored and never committed.\n"
            "  Re-running safely overwrites the file.\n"
            "  Obtain credentials: Google Cloud Console > APIs & Services > Credentials > OAuth 2.0 Client IDs."
        ),
    )
    parser.add_argument("--client-id", required=True, metavar="ID", help="Google OAuth client ID.")
    parser.add_argument("--client-secret", required=True, metavar="SECRET", help="Google OAuth client secret.")
    args = parser.parse_args()

    print("\nBankApp.Desktop — dev config setup\n------------------------------------")

    config = {
        "OAuth": {
            "Google": {
                "ClientId": args.client_id,
                "ClientSecret": args.client_secret,
            }
        }
    }

    OUTPUT_PATH.write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")

    print(f"[OK]   {OUTPUT_PATH.relative_to(REPO_ROOT)} written (gitignored).\n")


if __name__ == "__main__":
    main()
