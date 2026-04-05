#!/usr/bin/env python3
# setup-dev-config.py
# Creates src/BankApp.Client/appsettings.Local.json with OAuth credentials for local development.
# Requires Python 3.6+. No third-party packages needed.
#
# Run from the repo root:
#
#   python scripts/client/setup-dev-config.py
#
# appsettings.Local.json is gitignored and overrides appsettings.json locally.
# Re-running is safe — the file is fully overwritten each time.

import getpass
import json
import os
import sys

OUTPUT_PATH = os.path.join("src", "BankApp.Client", "appsettings.Local.json")


def main() -> None:
    print()
    print("BankApp.Client — dev config setup")
    print("-----------------------------------")
    print()
    print("OAuth credentials are required. Obtain them from the Google Cloud Console:")
    print("  console.cloud.google.com > APIs & Services > Credentials > OAuth 2.0 Client IDs")
    print()

    client_id = input("    OAuth:Google:ClientId     : ").strip()
    client_secret = getpass.getpass("    OAuth:Google:ClientSecret (input hidden): ")

    config = {
        "OAuth": {
            "Google": {
                "ClientId": client_id,
                "ClientSecret": client_secret,
            }
        }
    }

    with open(OUTPUT_PATH, "w") as f:
        json.dump(config, f, indent=2)
        f.write("\n")

    print()
    print(f"[OK] {OUTPUT_PATH} created.")
    print("     This file is gitignored and will never be committed.")
    print()


if __name__ == "__main__":
    main()
