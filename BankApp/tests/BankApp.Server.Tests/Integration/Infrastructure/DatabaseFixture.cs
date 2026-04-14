// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using BankApp.Server.DataAccess;
using Microsoft.Data.Sqlite;

namespace BankApp.Server.Tests.Integration.Infrastructure;

/// <summary>
/// Provides a repeatable SQLite in-memory database for integration tests.
/// Each test class that implements <see cref="IClassFixture{DatabaseFixture}"/>
/// shares one connection for the lifetime of that class.
/// Call <see cref="ResetAsync"/> before each test to wipe all data.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseFixture"/> class.
    /// Opens the in-memory connection and creates all tables from the schema.
    /// </summary>
    public DatabaseFixture()
    {
        // "Data Source=:memory:" keeps the DB alive as long as the connection is open.
        this.connection = new SqliteConnection("Data Source=:memory:");
        this.connection.Open();
        this.CreateSchema();
    }

    /// <summary>
    /// Creates a fresh <see cref="AppDbContext"/> (backed by SQLite) for use in a single test.
    /// </summary>
    /// <returns>A <see cref="SqliteDbContext"/> that wraps the shared in-memory connection.</returns>
    public AppDbContext CreateDbContext() => new SqliteDbContext(this.connection);

    /// <summary>
    /// Deletes all rows from every table in dependency-safe order.
    /// Call this in your test constructor or <c>BeforeEach</c> to avoid data contamination.
    /// </summary>
    public void Reset()
    {
        using var cmd = this.connection.CreateCommand();

        // Delete in reverse FK dependency order
        cmd.CommandText = """
            DELETE FROM TransactionCategoryOverride;
            DELETE FROM "Transaction";
            DELETE FROM Card;
            DELETE FROM Account;
            DELETE FROM Notification;
            DELETE FROM NotificationPreference;
            DELETE FROM PasswordResetToken;
            DELETE FROM OAuthLink;
            DELETE FROM "Session";
            DELETE FROM "User";
            DELETE FROM Category;
            """;
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.connection.Close();
        this.connection.Dispose();
    }

    // ─── Schema Creation ──────────────────────────────────────────────────────

    private void CreateSchema()
    {
        string schema = GetSqliteSchema();
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = schema;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Returns the SQLite-compatible DDL translated from the SQL Server schema.
    /// All T-SQL–specific types and syntax are substituted inline.
    /// </summary>
    private static string GetSqliteSchema()
    {
        // Schema expressed directly as SQLite DDL to avoid runtime translation of complex DDL.
        // Derived from scripts/db/schema/*.sql with the following mappings:
        //   INT IDENTITY(1,1) PRIMARY KEY  → INTEGER PRIMARY KEY AUTOINCREMENT
        //   DATETIME2 / DATE               → TEXT
        //   BIT                            → INTEGER
        //   DECIMAL(x,y)                   → REAL
        //   NVARCHAR / VARCHAR             → TEXT
        //   [Name]                         → "Name"
        //   FOREIGN KEY REFERENCES         → kept but not enforced by default in SQLite
        //   IF OBJECT_ID / GO              → removed
        return """
            CREATE TABLE IF NOT EXISTS "User" (
                Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                Email               TEXT    NOT NULL UNIQUE,
                PasswordHash        TEXT    NOT NULL,
                FullName            TEXT    NOT NULL,
                PhoneNumber         TEXT,
                DateOfBirth         TEXT,
                Address             TEXT,
                Nationality         TEXT,
                PreferredLanguage   TEXT    DEFAULT 'en',
                Is2FAEnabled        INTEGER DEFAULT 0,
                Preferred2FAMethod  TEXT,
                IsLocked            INTEGER DEFAULT 0,
                LockoutEnd          TEXT    NULL,
                FailedLoginAttempts INTEGER DEFAULT 0,
                CreatedAt           TEXT    DEFAULT (datetime('now')),
                UpdatedAt           TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS "Session" (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId       INTEGER NOT NULL REFERENCES "User"(Id),
                Token        TEXT    NOT NULL,
                DeviceInfo   TEXT,
                Browser      TEXT,
                IpAddress    TEXT,
                LastActiveAt TEXT,
                ExpiresAt    TEXT    NOT NULL,
                IsRevoked    INTEGER DEFAULT 0,
                CreatedAt    TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS OAuthLink (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId         INTEGER NOT NULL REFERENCES "User"(Id),
                Provider       TEXT    NOT NULL,
                ProviderUserId TEXT    NOT NULL,
                ProviderEmail  TEXT,
                LinkedAt       TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Account (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId      INTEGER NOT NULL REFERENCES "User"(Id),
                AccountName TEXT,
                IBAN        TEXT    NOT NULL UNIQUE,
                Currency    TEXT    NOT NULL,
                Balance     REAL    DEFAULT 0,
                AccountType TEXT    NOT NULL,
                Status      TEXT    DEFAULT 'Active',
                CreatedAt   TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Card (
                Id                    INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId             INTEGER NOT NULL REFERENCES Account(Id),
                UserId                INTEGER NOT NULL REFERENCES "User"(Id),
                CardNumber            TEXT    NOT NULL,
                CardholderName        TEXT    NOT NULL,
                ExpiryDate            TEXT    NOT NULL,
                CVV                   TEXT    NOT NULL,
                CardType              TEXT    NOT NULL,
                CardBrand             TEXT,
                Status                TEXT    DEFAULT 'Active',
                DailyTransactionLimit REAL,
                MonthlySpendingCap    REAL,
                AtmWithdrawalLimit    REAL,
                ContactlessLimit      REAL,
                IsContactlessEnabled  INTEGER DEFAULT 1,
                IsOnlineEnabled       INTEGER DEFAULT 1,
                SortOrder             INTEGER DEFAULT 0,
                CancelledAt           TEXT    NULL,
                CreatedAt             TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Category (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Name     TEXT    NOT NULL,
                Icon     TEXT,
                IsSystem INTEGER DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS "Transaction" (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId          INTEGER NOT NULL REFERENCES Account(Id),
                CardId             INTEGER NULL REFERENCES Card(Id),
                TransactionRef     TEXT    NOT NULL UNIQUE,
                Type               TEXT    NOT NULL,
                Direction          TEXT    NOT NULL,
                Amount             REAL    NOT NULL,
                Currency           TEXT    NOT NULL,
                BalanceAfter       REAL    NOT NULL,
                CounterpartyName   TEXT,
                CounterpartyIBAN   TEXT,
                MerchantName       TEXT,
                CategoryId         INTEGER NULL REFERENCES Category(Id),
                Description        TEXT,
                Fee                REAL    DEFAULT 0,
                ExchangeRate       REAL,
                Status             TEXT    NOT NULL,
                RelatedEntityType  TEXT,
                RelatedEntityId    INTEGER,
                CreatedAt          TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Notification (
                Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId            INTEGER NOT NULL REFERENCES "User"(Id),
                Title             TEXT    NOT NULL,
                Message           TEXT    NOT NULL,
                Type              TEXT    NOT NULL,
                Channel           TEXT    NOT NULL,
                IsRead            INTEGER DEFAULT 0,
                RelatedEntityType TEXT,
                RelatedEntityId   INTEGER,
                CreatedAt         TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS NotificationPreference (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId             INTEGER NOT NULL REFERENCES "User"(Id),
                Category           TEXT    NOT NULL,
                PushEnabled        INTEGER DEFAULT 1,
                EmailEnabled       INTEGER DEFAULT 1,
                SmsEnabled         INTEGER DEFAULT 0,
                MinAmountThreshold REAL
            );

            CREATE TABLE IF NOT EXISTS PasswordResetToken (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId    INTEGER NOT NULL REFERENCES "User"(Id),
                TokenHash TEXT    NOT NULL,
                ExpiresAt TEXT    NOT NULL,
                UsedAt    TEXT    NULL,
                CreatedAt TEXT    DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS TransactionCategoryOverride (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                TransactionId INTEGER NOT NULL REFERENCES "Transaction"(Id),
                UserId        INTEGER NOT NULL REFERENCES "User"(Id),
                CategoryId    INTEGER NOT NULL REFERENCES Category(Id)
            );
            """;
    }
}
