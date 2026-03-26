USE [MovieApp];
GO

-- Initialize UserSpins state for the Dummy User
DECLARE @UserId INT;

SELECT @UserId = Id
FROM dbo.Users
WHERE Username = N'Dummy User';

IF @UserId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.UserSpins WHERE UserId = @UserId)
    BEGIN
        INSERT INTO dbo.UserSpins (UserId, DailySpinsRemaining, BonusSpins, LoginStreak, EventSpinRewardsToday)
        VALUES (@UserId, 5, 2, 1, 0);
    END;
END;
GO
