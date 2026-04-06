#!/usr/bin/env python3
# Creates src/BankApp.Client/appsettings.Local.json with Google OAuth credentials.
# Run from the repo root: python scripts/client/setup-dev-config.py
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
    print("OAuth credentials required. Google Cloud Console > APIs & Services > Credentials > OAuth 2.0 Client IDs")
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
    print(f"[OK] {OUTPUT_PATH} written (gitignored).")
    print()


if __name__ == "__main__":
    main()
