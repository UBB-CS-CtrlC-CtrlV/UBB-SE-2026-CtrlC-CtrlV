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