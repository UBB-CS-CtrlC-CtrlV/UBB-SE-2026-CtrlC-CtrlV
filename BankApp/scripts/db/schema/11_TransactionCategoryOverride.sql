IF OBJECT_ID('dbo.TransactionCategoryOverride', 'U') IS NULL
CREATE TABLE TransactionCategoryOverride (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TransactionId INT NOT NULL FOREIGN KEY REFERENCES [Transaction](Id),
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Category(Id)
);
GO