USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.TriviaRewards', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TriviaRewards
    (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        IsRedeemed BIT NOT NULL
            CONSTRAINT DF_TriviaRewards_IsRedeemed DEFAULT (0),
        CreatedAt DATETIME NOT NULL
            CONSTRAINT DF_TriviaRewards_CreatedAt DEFAULT (GETDATE()),

        CONSTRAINT PK_TriviaRewards PRIMARY KEY (Id),
        CONSTRAINT FK_TriviaRewards_Users
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
GO