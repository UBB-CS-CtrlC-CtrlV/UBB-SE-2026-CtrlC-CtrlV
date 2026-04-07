USE BankAppDb; GO
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