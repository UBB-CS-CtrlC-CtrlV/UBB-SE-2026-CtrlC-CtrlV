// <copyright file="DatabaseFixture.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Infrastructure.Tests.Infrastructure;

/// <summary>
/// Provides isolated SQLite in-memory databases for integration tests.
/// Call <see cref="CreateContext"/> per test to get a fresh, schema-ready context.
/// </summary>
public class DatabaseFixture : IDisposable
{
    /// <summary>
    /// SQLite DDL compatible with the SQL Server schema in BankAppDatabaseSchema.sql.
    /// Key differences: AUTOINCREMENT, TEXT for strings/datetimes, INTEGER for BIT,
    /// NUMERIC for DECIMAL, and datetime('now') for GETUTCDATE().
    /// </summary>
    public static readonly string SchemaSql = @"
CREATE TABLE IF NOT EXISTS ""User"" (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    PhoneNumber TEXT,
    DateOfBirth TEXT,
    Address TEXT,
    Nationality TEXT,
    PreferredLanguage TEXT DEFAULT 'en',
    Is2FAEnabled INTEGER DEFAULT 0,
    Preferred2FAMethod TEXT,
    IsLocked INTEGER DEFAULT 0,
    LockoutEnd TEXT,
    FailedLoginAttempts INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS ""Session"" (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    Token TEXT NOT NULL,
    DeviceInfo TEXT,
    Browser TEXT,
    IpAddress TEXT,
    LastActiveAt TEXT,
    ExpiresAt TEXT NOT NULL,
    IsRevoked INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS OAuthLink (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    Provider TEXT NOT NULL,
    ProviderUserId TEXT NOT NULL,
    ProviderEmail TEXT,
    LinkedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Account (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    AccountName TEXT,
    IBAN TEXT NOT NULL UNIQUE,
    Currency TEXT NOT NULL,
    Balance NUMERIC DEFAULT 0,
    AccountType TEXT NOT NULL,
    Status TEXT DEFAULT 'Active',
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Card (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId INTEGER NOT NULL REFERENCES Account(Id),
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    CardNumber TEXT NOT NULL,
    CardholderName TEXT NOT NULL,
    ExpiryDate TEXT NOT NULL,
    CVV TEXT NOT NULL,
    CardType TEXT NOT NULL,
    CardBrand TEXT,
    Status TEXT DEFAULT 'Active',
    DailyTransactionLimit NUMERIC,
    MonthlySpendingCap NUMERIC,
    AtmWithdrawalLimit NUMERIC,
    ContactlessLimit NUMERIC,
    IsContactlessEnabled INTEGER DEFAULT 1,
    IsOnlineEnabled INTEGER DEFAULT 1,
    SortOrder INTEGER DEFAULT 0,
    CancelledAt TEXT,
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Category (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Icon TEXT,
    IsSystem INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS ""Transaction"" (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId INTEGER NOT NULL REFERENCES Account(Id),
    CardId INTEGER REFERENCES Card(Id),
    TransactionRef TEXT NOT NULL UNIQUE,
    Type TEXT NOT NULL,
    Direction TEXT NOT NULL,
    Amount NUMERIC NOT NULL,
    Currency TEXT NOT NULL,
    BalanceAfter NUMERIC NOT NULL,
    CounterpartyName TEXT,
    CounterpartyIBAN TEXT,
    MerchantName TEXT,
    CategoryId INTEGER REFERENCES Category(Id),
    Description TEXT,
    Fee NUMERIC DEFAULT 0,
    ExchangeRate NUMERIC,
    Status TEXT NOT NULL,
    RelatedEntityType TEXT,
    RelatedEntityId INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Notification (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    Title TEXT NOT NULL,
    Message TEXT NOT NULL,
    Type TEXT NOT NULL,
    Channel TEXT NOT NULL,
    IsRead INTEGER DEFAULT 0,
    RelatedEntityType TEXT,
    RelatedEntityId INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS NotificationPreference (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    Category TEXT NOT NULL,
    PushEnabled INTEGER DEFAULT 1,
    EmailEnabled INTEGER DEFAULT 1,
    SmsEnabled INTEGER DEFAULT 0,
    MinAmountThreshold NUMERIC
);

CREATE TABLE IF NOT EXISTS PasswordResetToken (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    TokenHash TEXT NOT NULL,
    ExpiresAt TEXT NOT NULL,
    UsedAt TEXT,
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS TransactionCategoryOverride (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TransactionId INTEGER NOT NULL REFERENCES ""Transaction""(Id),
    UserId INTEGER NOT NULL REFERENCES ""User""(Id),
    CategoryId INTEGER NOT NULL REFERENCES Category(Id)
);
";

    public void Dispose()
    {
        // Nothing shared to release – each context owns its connection.
    }

    /// <summary>
    /// Creates a fresh, isolated SQLite in-memory context with all tables created.
    /// The caller is responsible for disposing the returned context.
    /// </summary>
    public SqliteDbContext CreateContext()
    {
        // Unique database name per call → fully isolated database
        string dbName = $"testdb_{Guid.NewGuid():N}";
        string connStr = $"Data Source={dbName};Mode=Memory;Cache=Shared";

        var ctx = new SqliteDbContext(connStr);
        ctx.ExecuteSchema(SchemaSql);
        return ctx;
    }

    // ── Convenience seed helpers used by multiple test classes ─────────────

    /// <summary>Inserts a minimal User row and returns its auto-generated Id.</summary>
    public static int SeedUser(SqliteDbContext ctx, string email = "test@example.com", string fullName = "Test User")
    {
        var da = new UserDataAccess(ctx);
        var user = new User
        {
            Email = email,
            PasswordHash = "hash123",
            FullName = fullName,
            PreferredLanguage = "en",
        };
        da.Create(user);
        return da.FindByEmail(email)!.Id;
    }

    /// <summary>Inserts a minimal Account row and returns its Id.</summary>
    public static int SeedAccount(SqliteDbContext ctx, int userId, string iban = "RO49AAAA1B31007593840000")
    {
        ctx.ExecuteNonQuery(
            "INSERT INTO Account (UserId, IBAN, Currency, AccountType) VALUES (@p0, @p1, @p2, @p3)",
            new object[] { userId, iban, "RON", "Checking" });

        using var r = ctx.ExecuteQuery("SELECT last_insert_rowid()", Array.Empty<object>());
        r.Read();
        return r.GetInt32(0);
    }

    /// <summary>Inserts a minimal Card row and returns its Id.</summary>
    public static int SeedCard(SqliteDbContext ctx, int accountId, int userId)
    {
        ctx.ExecuteNonQuery(
            @"INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV, CardType)
              VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
            new object[] { accountId, userId, "4111111111111111", "Test User", "2028-12-31", "123", "Debit" });

        using var r = ctx.ExecuteQuery("SELECT last_insert_rowid()", Array.Empty<object>());
        r.Read();
        return r.GetInt32(0);
    }
}
