USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        UserId INT NOT NULL,
        EventId INT NOT NULL,
        Type NVARCHAR(64) NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        State NVARCHAR(16) NOT NULL
            CONSTRAINT DF_Notifications_State DEFAULT 'Unread',
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Notifications_CreatedAt DEFAULT GETUTCDATE(),

        CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Notifications_Events FOREIGN KEY (EventId)
            REFERENCES dbo.Events(Id) ON DELETE CASCADE
    );
END;
GO
