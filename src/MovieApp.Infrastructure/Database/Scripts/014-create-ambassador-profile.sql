USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.AmbassadorProfile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AmbassadorProfile
    (
        UserId INT NOT NULL,
        referral_code NVARCHAR(64) NOT NULL,
        reward_balance INT NOT NULL DEFAULT 0,
        CONSTRAINT PK_AmbassadorProfile PRIMARY KEY CLUSTERED (UserId),
        CONSTRAINT FK_AmbassadorProfile_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT UQ_AmbassadorProfile_ReferralCode UNIQUE (referral_code)
    );
END;
GO
