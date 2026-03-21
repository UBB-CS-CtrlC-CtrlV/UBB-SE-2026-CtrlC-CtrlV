USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.UserSpins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSpins
    (
        UserId INT NOT NULL,

        DailySpinsRemaining INT NOT NULL 
            CONSTRAINT DF_UserSpins_DailySpins DEFAULT (1),

        BonusSpins INT NOT NULL 
            CONSTRAINT DF_UserSpins_BonusSpins DEFAULT (0),

        LastSpinReset DATETIME NULL,

        LoginStreak INT NOT NULL 
            CONSTRAINT DF_UserSpins_LoginStreak DEFAULT (0),

        LastLoginDate DATETIME NULL,

        EventSpinRewardsToday INT NOT NULL 
            CONSTRAINT DF_UserSpins_EventRewards DEFAULT (0),

        CONSTRAINT PK_UserSpins PRIMARY KEY (UserId),

        CONSTRAINT FK_UserSpins_Users 
            FOREIGN KEY (UserId) 
            REFERENCES dbo.Users(Id)
    );
END;
GO