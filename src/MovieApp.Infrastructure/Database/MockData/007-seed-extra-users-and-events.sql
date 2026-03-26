USE [MovieApp];
GO

-- Seeds additional users and event inventory for demo scenarios.
-- This script is idempotent and safe to rerun after the main schema/bootstrap scripts.

;WITH MockUsers(AuthProvider, AuthSubject, Username) AS
(
    SELECT N'mock', N'ava-director', N'Ava Director' UNION ALL
    SELECT N'mock', N'noah-critic', N'Noah Critic' UNION ALL
    SELECT N'mock', N'mia-organizer', N'Mia Organizer' UNION ALL
    SELECT N'mock', N'liam-archivist', N'Liam Archivist' UNION ALL
    SELECT N'mock', N'zoe-viewer', N'Zoe Viewer' UNION ALL
    SELECT N'mock', N'ethan-ambassador', N'Ethan Ambassador' UNION ALL
    SELECT N'mock', N'sofia-marathoner', N'Sofia Marathoner' UNION ALL
    SELECT N'mock', N'lucas-collector', N'Lucas Collector'
)
INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
SELECT seed.AuthProvider, seed.AuthSubject, seed.Username
FROM MockUsers seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Users existing
    WHERE existing.AuthProvider = seed.AuthProvider
      AND existing.AuthSubject = seed.AuthSubject
);
GO

;WITH EventSeeds
(
    Title,
    Description,
    PosterUrl,
    DayOffset,
    LocationReference,
    TicketPrice,
    EventType,
    HistoricalRating,
    MaxCapacity,
    CurrentEnrollment,
    CreatorUsername
) AS
(
    SELECT N'Sunset Classics Screening', N'Golden-age favorites projected in restored format.', N'', 1, N'Cinema Hall B', CAST(12.50 AS DECIMAL(18,2)), N'Screening', CAST(4.1 AS FLOAT), 80, 26, N'Dummy User' UNION ALL
    SELECT N'Midnight Horror Marathon', N'Late-night crowd-pleasers with themed intermissions.', N'', 3, N'Underground Theater', CAST(35.00 AS DECIMAL(18,2)), N'Marathon', CAST(4.7 AS FLOAT), 120, 67, N'Ava Director' UNION ALL
    SELECT N'Directors Roundtable Live', N'Film-makers break down craft choices after the screening block.', N'', 5, N'Studio Loft', CAST(18.00 AS DECIMAL(18,2)), N'Special', CAST(4.6 AS FLOAT), 90, 41, N'Mia Organizer' UNION ALL
    SELECT N'Family Animation Morning', N'Accessible weekend programming for parents and kids.', N'', 6, N'Cityplex Room 3', CAST(9.00 AS DECIMAL(18,2)), N'Screening', CAST(4.3 AS FLOAT), 110, 58, N'Liam Archivist' UNION ALL
    SELECT N'Award Winners Spotlight', N'An evening built around recent award-season standouts.', N'', 8, N'Grand Theater', CAST(22.00 AS DECIMAL(18,2)), N'Premiere', CAST(4.9 AS FLOAT), 140, 102, N'Zoe Viewer' UNION ALL
    SELECT N'Sci-Fi Fan Meetup', N'A community screening with post-show discussion for genre fans.', N'', 10, N'Innovation Hub', CAST(14.00 AS DECIMAL(18,2)), N'Special', CAST(4.0 AS FLOAT), 100, 39, N'Noah Critic' UNION ALL
    SELECT N'Retro Action Weekend', N'A callback to practical-stunt era blockbusters.', N'', -4, N'Retro Cinema', CAST(28.00 AS DECIMAL(18,2)), N'Marathon', CAST(4.4 AS FLOAT), 95, 73, N'Ethan Ambassador' UNION ALL
    SELECT N'Open Air Romance Night', N'Outdoor courtyard screening with live music before showtime.', N'', -2, N'Riverside Courtyard', CAST(16.00 AS DECIMAL(18,2)), N'Screening', CAST(4.5 AS FLOAT), 130, 88, N'Zoe Viewer'
)
INSERT INTO dbo.Events
(
    Title,
    Description,
    PosterUrl,
    EventDateTime,
    LocationReference,
    TicketPrice,
    EventType,
    HistoricalRating,
    MaxCapacity,
    CurrentEnrollment,
    CreatorUserId
)
SELECT
    seed.Title,
    seed.Description,
    seed.PosterUrl,
    DATEADD(DAY, seed.DayOffset, GETDATE()),
    seed.LocationReference,
    seed.TicketPrice,
    seed.EventType,
    seed.HistoricalRating,
    seed.MaxCapacity,
    seed.CurrentEnrollment,
    creator.Id
FROM EventSeeds seed
INNER JOIN dbo.Users creator ON creator.Username = seed.CreatorUsername
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Events existing
    WHERE existing.Title = seed.Title
);
GO
