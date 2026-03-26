USE [MovieApp];
GO

-- Links events to movies and exposes active marathon/progress data
-- so slot-machine, event details, and marathon screens all have demo coverage.

;WITH ScreeningSeeds(EventTitle, MovieTitle, HourOffset) AS
(
    SELECT N'Cannes Winner Screening', N'Inception', 0 UNION ALL
    SELECT N'Vintage Film Marathon', N'Forrest Gump', 0 UNION ALL
    SELECT N'Vintage Film Marathon', N'Pulp Fiction', 3 UNION ALL
    SELECT N'Vintage Film Marathon', N'Gladiator', 6 UNION ALL
    SELECT N'Director''s Q&A: Sci-Fi Night', N'Interstellar', 0 UNION ALL
    SELECT N'Indie Documentary Showcase', N'Arrival', 0 UNION ALL
    SELECT N'Sunset Classics Screening', N'Forrest Gump', 0 UNION ALL
    SELECT N'Midnight Horror Marathon', N'Mad Max: Fury Road', 0 UNION ALL
    SELECT N'Directors Roundtable Live', N'Arrival', 0 UNION ALL
    SELECT N'Family Animation Morning', N'Barbie', 0 UNION ALL
    SELECT N'Award Winners Spotlight', N'La La Land', 0 UNION ALL
    SELECT N'Sci-Fi Fan Meetup', N'Blade Runner 2049', 0 UNION ALL
    SELECT N'Sci-Fi Fan Meetup', N'Avatar', 3 UNION ALL
    SELECT N'Retro Action Weekend', N'Mission Impossible', 0 UNION ALL
    SELECT N'Retro Action Weekend', N'The Avengers', 3 UNION ALL
    SELECT N'Open Air Romance Night', N'La La Land', 0
)
INSERT INTO dbo.Screenings (EventId, MovieId, ScreeningTime)
SELECT
    eventRow.Id,
    movieRow.Id,
    DATEADD(HOUR, seed.HourOffset, eventRow.EventDateTime)
FROM ScreeningSeeds seed
INNER JOIN dbo.Events eventRow ON eventRow.Title = seed.EventTitle
INNER JOIN dbo.Movies movieRow ON movieRow.Title = seed.MovieTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Screenings existing
    WHERE existing.EventId = eventRow.Id
      AND existing.MovieId = movieRow.Id
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = N'Awards Circuit')
BEGIN
    INSERT INTO dbo.Marathons
    (
        Title,
        Description,
        PosterUrl,
        Theme,
        PrerequisiteMarathonId,
        IsActive,
        LastFeaturedDate,
        WeekScoping
    )
    VALUES
    (
        N'Awards Circuit',
        N'A short-form marathon focused on critically celebrated recent films.',
        N'',
        N'Awards Season',
        NULL,
        1,
        GETDATE(),
        N'2026-W13'
    );
END;
GO

UPDATE dbo.Marathons
SET IsActive = CASE
        WHEN Title IN (N'Nolan Week', N'Sci-Fi Marathon', N'Action Legends', N'Awards Circuit') THEN 1
        ELSE IsActive
    END,
    LastFeaturedDate = CASE
        WHEN Title IN (N'Nolan Week', N'Sci-Fi Marathon', N'Action Legends', N'Awards Circuit') THEN GETDATE()
        ELSE LastFeaturedDate
    END,
    WeekScoping = CASE
        WHEN Title IN (N'Nolan Week', N'Sci-Fi Marathon', N'Action Legends', N'Awards Circuit') THEN N'2026-W13'
        ELSE WeekScoping
    END
WHERE Title IN (N'Nolan Week', N'Sci-Fi Marathon', N'Action Legends', N'Awards Circuit');
GO

;WITH AwardsCircuitMovieSeeds(MovieTitle) AS
(
    SELECT N'La La Land' UNION ALL
    SELECT N'Arrival'
)
INSERT INTO dbo.MarathonMovies (MarathonId, MovieId)
SELECT marathonRow.Id, movieRow.Id
FROM AwardsCircuitMovieSeeds seed
INNER JOIN dbo.Marathons marathonRow ON marathonRow.Title = N'Awards Circuit'
INNER JOIN dbo.Movies movieRow ON movieRow.Title = seed.MovieTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.MarathonMovies existing
    WHERE existing.MarathonId = marathonRow.Id
      AND existing.MovieId = movieRow.Id
);
GO

;WITH ProgressSeeds(Username, MarathonTitle, JoinedDaysAgo, TriviaAccuracy, CompletedMoviesCount, FinishedDaysAgo) AS
(
    SELECT N'Dummy User', N'Nolan Week', 9, CAST(100.0 AS FLOAT), 2, NULL UNION ALL
    SELECT N'Dummy User', N'Awards Circuit', 2, CAST(100.0 AS FLOAT), 1, NULL UNION ALL
    SELECT N'Sofia Marathoner', N'Nolan Week', 12, CAST(100.0 AS FLOAT), 4, 1 UNION ALL
    SELECT N'Noah Critic', N'Sci-Fi Marathon', 7, CAST(66.7 AS FLOAT), 2, NULL UNION ALL
    SELECT N'Ethan Ambassador', N'Action Legends', 5, CAST(100.0 AS FLOAT), 3, 2 UNION ALL
    SELECT N'Lucas Collector', N'Awards Circuit', 3, CAST(50.0 AS FLOAT), 1, NULL
)
INSERT INTO dbo.MarathonProgress
(
    UserId,
    MarathonId,
    JoinedAt,
    TriviaAccuracy,
    CompletedMoviesCount,
    FinishedAt
)
SELECT
    userRow.Id,
    marathonRow.Id,
    DATEADD(DAY, -seed.JoinedDaysAgo, GETDATE()),
    seed.TriviaAccuracy,
    seed.CompletedMoviesCount,
    CASE
        WHEN seed.FinishedDaysAgo IS NULL THEN NULL
        ELSE DATEADD(DAY, -seed.FinishedDaysAgo, GETDATE())
    END
FROM ProgressSeeds seed
INNER JOIN dbo.Users userRow ON userRow.Username = seed.Username
INNER JOIN dbo.Marathons marathonRow ON marathonRow.Title = seed.MarathonTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.MarathonProgress existing
    WHERE existing.UserId = userRow.Id
      AND existing.MarathonId = marathonRow.Id
);
GO
