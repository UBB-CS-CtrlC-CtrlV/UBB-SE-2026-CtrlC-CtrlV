USE [MovieApp];
GO

-- Adds relationship-heavy data so favorites, notifications, rewards,
-- referrals, and user state surfaces have meaningful demo content.

;WITH SpinSeeds
(
    Username,
    DailySpinsRemaining,
    BonusSpins,
    LastSlotSpinReset,
    LastTriviaSpinReset,
    LoginStreak,
    LastLoginDate,
    EventSpinRewardsToday
) AS
(
    SELECT N'Dummy User', 5, 2, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, -1, GETDATE()), 2, DATEADD(DAY, -1, GETDATE()), 1 UNION ALL
    SELECT N'Ava Director', 4, 1, GETDATE(), GETDATE(), 4, GETDATE(), 0 UNION ALL
    SELECT N'Noah Critic', 3, 0, GETDATE(), GETDATE(), 1, GETDATE(), 0 UNION ALL
    SELECT N'Mia Organizer', 5, 0, GETDATE(), GETDATE(), 5, GETDATE(), 0 UNION ALL
    SELECT N'Liam Archivist', 2, 1, GETDATE(), GETDATE(), 3, GETDATE(), 1 UNION ALL
    SELECT N'Zoe Viewer', 5, 1, GETDATE(), GETDATE(), 2, GETDATE(), 0 UNION ALL
    SELECT N'Ethan Ambassador', 5, 3, GETDATE(), GETDATE(), 6, GETDATE(), 0 UNION ALL
    SELECT N'Sofia Marathoner', 4, 2, GETDATE(), GETDATE(), 4, GETDATE(), 0 UNION ALL
    SELECT N'Lucas Collector', 3, 1, GETDATE(), GETDATE(), 2, GETDATE(), 0
)
INSERT INTO dbo.UserSpins
(
    UserId,
    DailySpinsRemaining,
    BonusSpins,
    LastSlotSpinReset,
    LastTriviaSpinReset,
    LoginStreak,
    LastLoginDate,
    EventSpinRewardsToday
)
SELECT
    userRow.Id,
    seed.DailySpinsRemaining,
    seed.BonusSpins,
    seed.LastSlotSpinReset,
    seed.LastTriviaSpinReset,
    seed.LoginStreak,
    seed.LastLoginDate,
    seed.EventSpinRewardsToday
FROM SpinSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.UserSpins existing
    WHERE existing.UserId = userRow.Id
);
GO

;WITH ParticipationSeeds(Username, EventTitle, Status, JoinedDaysAgo) AS
(
    SELECT N'Dummy User', N'Cannes Winner Screening', N'Confirmed', 7 UNION ALL
    SELECT N'Dummy User', N'Sunset Classics Screening', N'Confirmed', 2 UNION ALL
    SELECT N'Dummy User', N'Award Winners Spotlight', N'Confirmed', 1 UNION ALL
    SELECT N'Noah Critic', N'Director''s Q&A: Sci-Fi Night', N'Confirmed', 4 UNION ALL
    SELECT N'Noah Critic', N'Sci-Fi Fan Meetup', N'Confirmed', 2 UNION ALL
    SELECT N'Mia Organizer', N'Directors Roundtable Live', N'Confirmed', 3 UNION ALL
    SELECT N'Liam Archivist', N'Family Animation Morning', N'Confirmed', 1 UNION ALL
    SELECT N'Zoe Viewer', N'Award Winners Spotlight', N'Confirmed', 2 UNION ALL
    SELECT N'Ethan Ambassador', N'Retro Action Weekend', N'CheckedIn', 8 UNION ALL
    SELECT N'Sofia Marathoner', N'Vintage Film Marathon', N'Confirmed', 5 UNION ALL
    SELECT N'Sofia Marathoner', N'Midnight Horror Marathon', N'Confirmed', 1 UNION ALL
    SELECT N'Lucas Collector', N'Open Air Romance Night', N'CheckedIn', 6
)
INSERT INTO dbo.Participations (UserId, EventId, Status, JoinedAt)
SELECT
    userRow.Id,
    eventRow.Id,
    seed.Status,
    DATEADD(DAY, -seed.JoinedDaysAgo, GETUTCDATE())
FROM ParticipationSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
INNER JOIN dbo.Events eventRow ON eventRow.Title = seed.EventTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Participations existing
    WHERE existing.UserId = userRow.Id
      AND existing.EventId = eventRow.Id
);
GO

;WITH FavoriteSeeds(Username, EventTitle, CreatedDaysAgo) AS
(
    SELECT N'Dummy User', N'Cannes Winner Screening', 8 UNION ALL
    SELECT N'Dummy User', N'Sci-Fi Fan Meetup', 3 UNION ALL
    SELECT N'Dummy User', N'Award Winners Spotlight', 2 UNION ALL
    SELECT N'Zoe Viewer', N'Open Air Romance Night', 5 UNION ALL
    SELECT N'Lucas Collector', N'Sunset Classics Screening', 4
)
INSERT INTO dbo.FavoriteEvents (UserId, EventId, CreatedAt)
SELECT
    userRow.Id,
    eventRow.Id,
    DATEADD(DAY, -seed.CreatedDaysAgo, GETUTCDATE())
FROM FavoriteSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
INNER JOIN dbo.Events eventRow ON eventRow.Title = seed.EventTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.FavoriteEvents existing
    WHERE existing.UserId = userRow.Id
      AND existing.EventId = eventRow.Id
);
GO

;WITH NotificationSeeds(Username, EventTitle, Type, Message, State, CreatedHoursAgo) AS
(
    SELECT N'Dummy User', N'Cannes Winner Screening', N'FavoriteReminder', N'Your saved premiere starts soon.', N'Unread', 2 UNION ALL
    SELECT N'Dummy User', N'Sci-Fi Fan Meetup', N'FavoriteReminder', N'Your favorite sci-fi meetup has new seats available.', N'Unread', 12 UNION ALL
    SELECT N'Dummy User', N'Award Winners Spotlight', N'EventUpdate', N'The spotlight event added a post-show discussion.', N'Read', 30 UNION ALL
    SELECT N'Zoe Viewer', N'Open Air Romance Night', N'FavoriteReminder', N'The outdoor screening is still on for tonight.', N'Unread', 5
)
INSERT INTO dbo.Notifications (UserId, EventId, Type, Message, State, CreatedAt)
SELECT
    userRow.Id,
    eventRow.Id,
    seed.Type,
    seed.Message,
    seed.State,
    DATEADD(HOUR, -seed.CreatedHoursAgo, GETUTCDATE())
FROM NotificationSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
INNER JOIN dbo.Events eventRow ON eventRow.Title = seed.EventTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Notifications existing
    WHERE existing.UserId = userRow.Id
      AND existing.EventId = eventRow.Id
      AND existing.Message = seed.Message
);
GO

;WITH DiscountSeeds(Username, MovieTitle, DiscountPercentage, CreatedDaysAgo) AS
(
    SELECT N'Dummy User', N'Inception', CAST(10.00 AS DECIMAL(5,2)), 1 UNION ALL
    SELECT N'Dummy User', N'Arrival', CAST(15.00 AS DECIMAL(5,2)), 2 UNION ALL
    SELECT N'Ethan Ambassador', N'Gladiator', CAST(10.00 AS DECIMAL(5,2)), 1
)
INSERT INTO dbo.UserMovieDiscounts (UserId, MovieId, DiscountPercentage, CreatedAt)
SELECT
    userRow.Id,
    movieRow.Id,
    seed.DiscountPercentage,
    DATEADD(DAY, -seed.CreatedDaysAgo, GETDATE())
FROM DiscountSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
INNER JOIN dbo.Movies movieRow ON movieRow.Title = seed.MovieTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.UserMovieDiscounts existing
    WHERE existing.UserId = userRow.Id
      AND existing.MovieId = movieRow.Id
      AND existing.DiscountPercentage = seed.DiscountPercentage
);
GO

;WITH AmbassadorSeeds(Username, ReferralCode, RewardBalance) AS
(
    SELECT N'Dummy User', N'DUMMY-FILM-01', 2 UNION ALL
    SELECT N'Ethan Ambassador', N'ETHAN-CLUB-02', 1
)
INSERT INTO dbo.AmbassadorProfile (UserId, referral_code, reward_balance)
SELECT userRow.Id, seed.ReferralCode, seed.RewardBalance
FROM AmbassadorSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.AmbassadorProfile existing
    WHERE existing.UserId = userRow.Id
);
GO

;WITH ReferralSeeds(AmbassadorUsername, FriendUsername, EventTitle, UsedDaysAgo) AS
(
    SELECT N'Dummy User', N'Zoe Viewer', N'Award Winners Spotlight', 3 UNION ALL
    SELECT N'Dummy User', N'Lucas Collector', N'Sunset Classics Screening', 2 UNION ALL
    SELECT N'Ethan Ambassador', N'Sofia Marathoner', N'Midnight Horror Marathon', 1
)
INSERT INTO dbo.ReferralLog (AmbassadorID, FriendID, EventID, UsedAt)
SELECT
    ambassador.Id,
    friend.Id,
    eventRow.Id,
    DATEADD(DAY, -seed.UsedDaysAgo, SYSUTCDATETIME())
FROM ReferralSeeds seed
INNER JOIN dbo.Users ambassador ON ambassador.Username = seed.AmbassadorUsername
INNER JOIN dbo.Users friend ON friend.Username = seed.FriendUsername
INNER JOIN dbo.Events eventRow ON eventRow.Title = seed.EventTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ReferralLog existing
    WHERE existing.AmbassadorID = ambassador.Id
      AND existing.FriendID = friend.Id
      AND existing.EventID = eventRow.Id
);
GO

;WITH TriviaRewardSeeds(Username, IsRedeemed, CreatedDaysAgo) AS
(
    SELECT N'Dummy User', CAST(0 AS BIT), 1 UNION ALL
    SELECT N'Noah Critic', CAST(1 AS BIT), 6
)
INSERT INTO dbo.TriviaRewards (UserId, IsRedeemed, CreatedAt)
SELECT
    userRow.Id,
    seed.IsRedeemed,
    DATEADD(DAY, -seed.CreatedDaysAgo, GETDATE())
FROM TriviaRewardSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.TriviaRewards existing
    WHERE existing.UserId = userRow.Id
      AND existing.IsRedeemed = seed.IsRedeemed
);
GO
