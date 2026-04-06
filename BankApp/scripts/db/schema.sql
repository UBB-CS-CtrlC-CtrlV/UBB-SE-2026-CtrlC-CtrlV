IF DB_ID('BankAppDb') IS NULL
    CREATE DATABASE BankAppDb;
GO

USE BankAppDb;
GO

IF OBJECT_ID('dbo.[User]', 'U') IS NULL
CREATE TABLE [User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(512) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    PhoneNumber VARCHAR(20),
    DateOfBirth DATE,
    [Address] NVARCHAR(MAX),
    Nationality VARCHAR(100),
    PreferredLanguage VARCHAR(5) DEFAULT 'en',
    Is2FAEnabled BIT DEFAULT 0,
    Preferred2FAMethod VARCHAR(20),
    IsLocked BIT DEFAULT 0,
    LockoutEnd DATETIME2 NULL,
    FailedLoginAttempts INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.[Session]', 'U') IS NULL
CREATE TABLE [Session] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Token VARCHAR(512) NOT NULL,
    DeviceInfo VARCHAR(255),
    Browser VARCHAR(100),
    IpAddress VARCHAR(45),
    LastActiveAt DATETIME2,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.OAuthLink', 'U') IS NULL
CREATE TABLE OAuthLink (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Provider VARCHAR(20) NOT NULL,
    ProviderUserId VARCHAR(255) NOT NULL,
    ProviderEmail VARCHAR(255),
    LinkedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.Account', 'U') IS NULL
CREATE TABLE Account (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    AccountName VARCHAR(100),
    IBAN VARCHAR(34) NOT NULL UNIQUE,
    Currency VARCHAR(3) NOT NULL,
    Balance DECIMAL(18,2) DEFAULT 0,
    AccountType VARCHAR(20) NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.Card', 'U') IS NULL
CREATE TABLE Card (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL FOREIGN KEY REFERENCES Account(Id),
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    CardNumber VARCHAR(19) NOT NULL,
    CardholderName NVARCHAR(200) NOT NULL,
    ExpiryDate DATE NOT NULL,
    CVV VARCHAR(4) NOT NULL,
    CardType VARCHAR(20) NOT NULL,
    CardBrand VARCHAR(20),
    Status VARCHAR(20) DEFAULT 'Active',
    DailyTransactionLimit DECIMAL(18,2),
    MonthlySpendingCap DECIMAL(18,2),
    AtmWithdrawalLimit DECIMAL(18,2),
    ContactlessLimit DECIMAL(18,2),
    IsContactlessEnabled BIT DEFAULT 1,
    IsOnlineEnabled BIT DEFAULT 1,
    SortOrder INT DEFAULT 0,
    CancelledAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.Category', 'U') IS NULL
CREATE TABLE Category (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Icon VARCHAR(50),
    IsSystem BIT DEFAULT 1
);
GO

IF OBJECT_ID('dbo.[Transaction]', 'U') IS NULL
CREATE TABLE [Transaction] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL FOREIGN KEY REFERENCES Account(Id),
    CardId INT NULL FOREIGN KEY REFERENCES Card(Id),
    TransactionRef VARCHAR(50) NOT NULL UNIQUE,
    [Type] VARCHAR(30) NOT NULL,
    Direction VARCHAR(10) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(3) NOT NULL,
    BalanceAfter DECIMAL(18,2) NOT NULL,
    CounterpartyName NVARCHAR(200),
    CounterpartyIBAN VARCHAR(34),
    MerchantName NVARCHAR(200),
    CategoryId INT NULL FOREIGN KEY REFERENCES Category(Id),
    [Description] NVARCHAR(MAX),
    Fee DECIMAL(18,2) DEFAULT 0,
    ExchangeRate DECIMAL(18,6),
    Status VARCHAR(20) NOT NULL,
    RelatedEntityType VARCHAR(50),
    RelatedEntityId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.Notification', 'U') IS NULL
CREATE TABLE Notification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Title NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [Type] VARCHAR(30) NOT NULL,
    Channel VARCHAR(20) NOT NULL,
    IsRead BIT DEFAULT 0,
    RelatedEntityType VARCHAR(50),
    RelatedEntityId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.NotificationPreference', 'U') IS NULL
CREATE TABLE NotificationPreference (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Category VARCHAR(30) NOT NULL,
    PushEnabled BIT DEFAULT 1,
    EmailEnabled BIT DEFAULT 1,
    SmsEnabled BIT DEFAULT 0,
    MinAmountThreshold DECIMAL(18,2)
);
GO

IF OBJECT_ID('dbo.PasswordResetToken', 'U') IS NULL
CREATE TABLE PasswordResetToken (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    TokenHash VARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('dbo.TransactionCategoryOverride', 'U') IS NULL
CREATE TABLE TransactionCategoryOverride (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TransactionId INT NOT NULL FOREIGN KEY REFERENCES [Transaction](Id),
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Category(Id)
);
GO
