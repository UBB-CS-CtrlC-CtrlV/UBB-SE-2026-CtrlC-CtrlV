USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.ReferralLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReferralLog
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        AmbassadorID INT NOT NULL,
        FriendID INT NOT NULL,
        EventID INT NOT NULL,
        UsedAt DATETIME2 NOT NULL CONSTRAINT DF_ReferralLog_UsedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ReferralLog PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_ReferralLog_Ambassador FOREIGN KEY (AmbassadorID) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_ReferralLog_Friend FOREIGN KEY (FriendID) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_ReferralLog_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(Id)
    );
END;
GO
