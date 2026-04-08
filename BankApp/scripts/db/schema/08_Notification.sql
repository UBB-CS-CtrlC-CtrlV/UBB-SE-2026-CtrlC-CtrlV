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