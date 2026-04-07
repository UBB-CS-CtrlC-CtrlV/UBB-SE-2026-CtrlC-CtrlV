USE BankAppDb; GO
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