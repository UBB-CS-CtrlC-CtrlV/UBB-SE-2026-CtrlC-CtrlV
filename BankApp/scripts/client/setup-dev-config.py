#!/usr/bin/env python3
# setup-dev-config.py
# Creates src/BankApp.Client/appsettings.Local.json with local development configuration.
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

DEFAULT_API_BASE_URL = "http://localhost:5024"
DEFAULT_REDIRECT_URI = "http://127.0.0.1:7890/"


def main() -> None:
    print()
    print("BankApp.Client — dev config setup")
    print("-----------------------------------")

    # --- API base URL ---
    print()
    print(f"Default API base URL: {DEFAULT_API_BASE_URL}")
    api_url = input("    ApiBaseUrl (press Enter to accept default): ").strip()
    if not api_url:
        api_url = DEFAULT_API_BASE_URL

    # --- OAuth redirect URI ---
    print()
    print(f"Default OAuth redirect URI: {DEFAULT_REDIRECT_URI}")
    redirect_uri = input("    OAuth:Google:RedirectUri (press Enter to accept default): ").strip()
    if not redirect_uri:
        redirect_uri = DEFAULT_REDIRECT_URI

    # --- OAuth credentials ---
    print()
    print("OAuth credentials are required. Obtain them from the Google Cloud Console:")
    print("  console.cloud.google.com > APIs & Services > Credentials > OAuth 2.0 Client IDs")
    print()

    client_id = input("    OAuth:Google:ClientId     : ").strip()
    client_secret = getpass.getpass("    OAuth:Google:ClientSecret (input hidden): ")

    config = {
        "ApiBaseUrl": api_url,
        "OAuth": {
            "Google": {
                "RedirectUri": redirect_uri,
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
