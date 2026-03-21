USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.FavoriteEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FavoriteEvents
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        UserId INT NOT NULL,
        EventId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_FavoriteEvents_CreatedAt DEFAULT GETUTCDATE(),

        CONSTRAINT PK_FavoriteEvents PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT UQ_FavoriteEvents_User_Event UNIQUE (UserId, EventId),

        CONSTRAINT FK_FavoriteEvents_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users(Id),
        CONSTRAINT FK_FavoriteEvents_Events FOREIGN KEY (EventId)
            REFERENCES dbo.Events(Id) ON DELETE CASCADE
    );
END;
GO
