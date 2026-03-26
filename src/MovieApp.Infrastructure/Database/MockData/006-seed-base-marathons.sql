USE [MovieApp];
GO


IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Nolan Week')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Nolan Week', 'Watch and verify Christopher Nolan films.', '', 'Christopher Nolan', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Sci-Fi Marathon')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Sci-Fi Marathon', 'Explore the best science fiction films.', '', 'Science Fiction', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Thriller Night')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Thriller Night', 'Suspense and thrills back to back.', '', 'Thriller', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Drama Classics')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Drama Classics', 'The greatest dramatic performances on screen.', '', 'Drama', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Action Legends')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Action Legends', 'Non-stop action from the biggest names.', '', 'Action', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'DiCaprio Spotlight')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('DiCaprio Spotlight', 'Every great Leonardo DiCaprio performance.', '', 'Leonardo DiCaprio', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Scorsese Collection')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Scorsese Collection', 'Martin Scorsese masterpieces.', '', 'Martin Scorsese', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Tarantino Universe')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Tarantino Universe', 'Quentin Tarantino films from start to finish.', '', 'Quentin Tarantino', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Ridley Scott Epics')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Ridley Scott Epics', 'Epic storytelling from Ridley Scott.', '', 'Ridley Scott', NULL, 0, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Marathons WHERE Title = 'Spielberg Classics')
BEGIN
    INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, IsActive, WeekScoping)
    VALUES ('Spielberg Classics', 'Timeless classics directed by Steven Spielberg.', '', 'Steven Spielberg', NULL, 0, NULL);
END;

GO

DECLARE @NolanId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Nolan Week');
DECLARE @SciFiId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Sci-Fi Marathon');
DECLARE @ThrillerID INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Thriller Night');
DECLARE @DramaId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Drama Classics');
DECLARE @ActionId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Action Legends');
DECLARE @DiCaprioId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'DiCaprio Spotlight');
DECLARE @ScorseseId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Scorsese Collection');
DECLARE @TarantinoId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Tarantino Universe');
DECLARE @RidleyId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Ridley Scott Epics');
DECLARE @SpielbergId INT = (SELECT Id FROM dbo.Marathons WHERE Title = 'Spielberg Classics');

DECLARE @Inception INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Inception');
DECLARE @DarkKnight INT = (SELECT Id FROM dbo.Movies WHERE Title = 'The Dark Knight');
DECLARE @Interstellar INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Interstellar');
DECLARE @Dunkirk INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Dunkirk');
DECLARE @Avatar INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Avatar');
DECLARE @BladeRunner INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Blade Runner 2049');
DECLARE @Avengers INT = (SELECT Id FROM dbo.Movies WHERE Title = 'The Avengers');
DECLARE @MissionImpossible INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Mission Impossible');
DECLARE @WolfWallStreet INT = (SELECT Id FROM dbo.Movies WHERE Title = 'The Wolf of Wall Street');
DECLARE @PulpFiction INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Pulp Fiction');
DECLARE @Gladiator INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Gladiator');
DECLARE @ForrestGump INT = (SELECT Id FROM dbo.Movies WHERE Title = 'Forrest Gump');

IF @Inception IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @NolanId AND MovieId = @Inception)
    INSERT INTO dbo.MarathonMovies VALUES (@NolanId, @Inception);
IF @DarkKnight IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @NolanId AND MovieId = @DarkKnight)
    INSERT INTO dbo.MarathonMovies VALUES (@NolanId, @DarkKnight);
IF @Interstellar IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @NolanId AND MovieId = @Interstellar)
    INSERT INTO dbo.MarathonMovies VALUES (@NolanId, @Interstellar);
IF @Dunkirk IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @NolanId AND MovieId = @Dunkirk)
    INSERT INTO dbo.MarathonMovies VALUES (@NolanId, @Dunkirk);

IF @Inception IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @SciFiId AND MovieId = @Inception)
    INSERT INTO dbo.MarathonMovies VALUES (@SciFiId, @Inception);
IF @Avatar IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @SciFiId AND MovieId = @Avatar)
    INSERT INTO dbo.MarathonMovies VALUES (@SciFiId, @Avatar);
IF @BladeRunner IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @SciFiId AND MovieId = @BladeRunner)
    INSERT INTO dbo.MarathonMovies VALUES (@SciFiId, @BladeRunner);
IF @Interstellar IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @SciFiId AND MovieId = @Interstellar)
    INSERT INTO dbo.MarathonMovies VALUES (@SciFiId, @Interstellar);

IF @DarkKnight IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ThrillerID AND MovieId = @DarkKnight)
    INSERT INTO dbo.MarathonMovies VALUES (@ThrillerID, @DarkKnight);
IF @PulpFiction IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ThrillerID AND MovieId = @PulpFiction)
    INSERT INTO dbo.MarathonMovies VALUES (@ThrillerID, @PulpFiction);
IF @BladeRunner IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ThrillerID AND MovieId = @BladeRunner)
    INSERT INTO dbo.MarathonMovies VALUES (@ThrillerID, @BladeRunner);

IF @ForrestGump IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @DramaId AND MovieId = @ForrestGump)
    INSERT INTO dbo.MarathonMovies VALUES (@DramaId, @ForrestGump);
IF @WolfWallStreet IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @DramaId AND MovieId = @WolfWallStreet)
    INSERT INTO dbo.MarathonMovies VALUES (@DramaId, @WolfWallStreet);
IF @Gladiator IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @DramaId AND MovieId = @Gladiator)
    INSERT INTO dbo.MarathonMovies VALUES (@DramaId, @Gladiator);

IF @Avengers IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ActionId AND MovieId = @Avengers)
    INSERT INTO dbo.MarathonMovies VALUES (@ActionId, @Avengers);
IF @MissionImpossible IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ActionId AND MovieId = @MissionImpossible)
    INSERT INTO dbo.MarathonMovies VALUES (@ActionId, @MissionImpossible);
IF @Gladiator IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ActionId AND MovieId = @Gladiator)
    INSERT INTO dbo.MarathonMovies VALUES (@ActionId, @Gladiator);

IF @Inception IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @DiCaprioId AND MovieId = @Inception)
    INSERT INTO dbo.MarathonMovies VALUES (@DiCaprioId, @Inception);
IF @WolfWallStreet IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @DiCaprioId AND MovieId = @WolfWallStreet)
    INSERT INTO dbo.MarathonMovies VALUES (@DiCaprioId, @WolfWallStreet);
IF @Interstellar IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @DiCaprioId AND MovieId = @Interstellar)
    INSERT INTO dbo.MarathonMovies VALUES (@DiCaprioId, @Interstellar);

IF @WolfWallStreet IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @ScorseseId AND MovieId = @WolfWallStreet)
    INSERT INTO dbo.MarathonMovies VALUES (@ScorseseId, @WolfWallStreet);

IF @PulpFiction IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @TarantinoId AND MovieId = @PulpFiction)
    INSERT INTO dbo.MarathonMovies VALUES (@TarantinoId, @PulpFiction);

IF @Gladiator IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @RidleyId AND MovieId = @Gladiator)
    INSERT INTO dbo.MarathonMovies VALUES (@RidleyId, @Gladiator);
IF @BladeRunner IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @RidleyId AND MovieId = @BladeRunner)
    INSERT INTO dbo.MarathonMovies VALUES (@RidleyId, @BladeRunner);

IF @MissionImpossible IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @SpielbergId AND MovieId = @MissionImpossible)
    INSERT INTO dbo.MarathonMovies VALUES (@SpielbergId, @MissionImpossible);
IF @ForrestGump IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MarathonMovies WHERE MarathonId = @SpielbergId AND MovieId = @ForrestGump)
    INSERT INTO dbo.MarathonMovies VALUES (@SpielbergId, @ForrestGump);
GO