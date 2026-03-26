USE [MovieApp];
GO

-- =============================================
-- Extra users for leaderboard testing
-- =============================================

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-alice')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-alice', 'Alice');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-bob')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-bob', 'Bob');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-carol')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-carol', 'Carol');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-dan')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-dan', 'Dan');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-eva')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-eva', 'Eva');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-frank')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-frank', 'Frank');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-grace')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-grace', 'Grace');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE AuthSubject = 'user-henry')
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES ('dummy', 'user-henry', 'Henry');
GO

-- =============================================
-- Movie-specific trivia questions (3 per movie)
-- =============================================

DECLARE @Inception INT        = (SELECT Id FROM dbo.Movies WHERE Title = 'Inception');
DECLARE @DarkKnight INT       = (SELECT Id FROM dbo.Movies WHERE Title = 'The Dark Knight');
DECLARE @Interstellar INT     = (SELECT Id FROM dbo.Movies WHERE Title = 'Interstellar');
DECLARE @Dunkirk INT          = (SELECT Id FROM dbo.Movies WHERE Title = 'Dunkirk');
DECLARE @Avatar INT           = (SELECT Id FROM dbo.Movies WHERE Title = 'Avatar');
DECLARE @BladeRunner INT      = (SELECT Id FROM dbo.Movies WHERE Title = 'Blade Runner 2049');
DECLARE @Avengers INT         = (SELECT Id FROM dbo.Movies WHERE Title = 'The Avengers');
DECLARE @MissionImp INT       = (SELECT Id FROM dbo.Movies WHERE Title = 'Mission Impossible');
DECLARE @WolfWallSt INT       = (SELECT Id FROM dbo.Movies WHERE Title = 'The Wolf of Wall Street');
DECLARE @PulpFiction INT      = (SELECT Id FROM dbo.Movies WHERE Title = 'Pulp Fiction');
DECLARE @Gladiator INT        = (SELECT Id FROM dbo.Movies WHERE Title = 'Gladiator');
DECLARE @ForrestGump INT      = (SELECT Id FROM dbo.Movies WHERE Title = 'Forrest Gump');

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @Inception)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who directed Inception?',
     'General Movie Trivia',
     'Steven Spielberg', 'Christopher Nolan', 'James Cameron', 'Ridley Scott',
     'B', @Inception),
    ('What is the name of the spinning top used as a totem in Inception?',
     'General Movie Trivia',
     'The Token', 'The Totem', 'The Spinner', 'The Dream Key',
     'B', @Inception),
    ('Which actor plays Dom Cobb in Inception?',
     'General Movie Trivia',
     'Matt Damon', 'Brad Pitt', 'Leonardo DiCaprio', 'Tom Hanks',
     'C', @Inception);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @DarkKnight)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who plays the Joker in The Dark Knight?',
     'General Movie Trivia',
     'Jack Nicholson', 'Jared Leto', 'Heath Ledger', 'Joaquin Phoenix',
     'C', @DarkKnight),
    ('What is Batman''s real name in The Dark Knight?',
     'General Movie Trivia',
     'Bruce Banner', 'Bruce Wayne', 'Clark Kent', 'Tony Stark',
     'B', @DarkKnight),
    ('Which city does Batman protect in The Dark Knight?',
     'General Movie Trivia',
     'Metropolis', 'Star City', 'Gotham City', 'Central City',
     'C', @DarkKnight);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @Interstellar)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('What is the name of the robot companion in Interstellar?',
     'General Movie Trivia',
     'HAL', 'TARS', 'WALL-E', 'R2D2',
     'B', @Interstellar),
    ('Which actor plays Cooper in Interstellar?',
     'General Movie Trivia',
     'Matt Damon', 'Brad Pitt', 'Matthew McConaughey', 'Tom Hanks',
     'C', @Interstellar),
    ('What celestial phenomenon is central to Interstellar?',
     'General Movie Trivia',
     'Neutron Star', 'Black Hole', 'Supernova', 'Wormhole Only',
     'B', @Interstellar);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @Dunkirk)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Dunkirk is set during which war?',
     'General Movie Trivia',
     'World War I', 'World War II', 'The Korean War', 'The Vietnam War',
     'B', @Dunkirk),
    ('Who directed Dunkirk?',
     'General Movie Trivia',
     'Steven Spielberg', 'Ridley Scott', 'Christopher Nolan', 'Michael Bay',
     'C', @Dunkirk),
    ('Dunkirk takes place primarily at what location?',
     'General Movie Trivia',
     'Normandy Beach', 'Dunkirk Beach', 'Dover Cliffs', 'The English Channel',
     'B', @Dunkirk);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @Avatar)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('What is the name of the alien planet in Avatar?',
     'General Movie Trivia',
     'Endor', 'Pandora', 'Kepler', 'Titan',
     'B', @Avatar),
    ('Who directed Avatar?',
     'General Movie Trivia',
     'Steven Spielberg', 'Peter Jackson', 'James Cameron', 'Michael Bay',
     'C', @Avatar),
    ('What is the precious mineral being mined on Pandora in Avatar?',
     'General Movie Trivia',
     'Dilithium', 'Unobtanium', 'Vibranium', 'Adamantium',
     'B', @Avatar);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @BladeRunner)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Which actor plays the lead in Blade Runner 2049?',
     'General Movie Trivia',
     'Chris Evans', 'Ryan Gosling', 'Matt Damon', 'Jake Gyllenhaal',
     'B', @BladeRunner),
    ('Who directed Blade Runner 2049?',
     'General Movie Trivia',
     'Ridley Scott', 'Christopher Nolan', 'Denis Villeneuve', 'David Fincher',
     'C', @BladeRunner),
    ('Blade Runner 2049 is set in which year?',
     'General Movie Trivia',
     '2019', '2035', '2049', '2077',
     'C', @BladeRunner);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @Avengers)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who assembles the Avengers in the first film?',
     'General Movie Trivia',
     'Tony Stark', 'Nick Fury', 'Steve Rogers', 'Thor',
     'B', @Avengers),
    ('What is the villain''s name in The Avengers 2012?',
     'General Movie Trivia',
     'Ultron', 'Thanos', 'Loki', 'Ronan',
     'C', @Avengers),
    ('Which gem does Loki possess in The Avengers?',
     'General Movie Trivia',
     'The Space Stone', 'The Mind Stone', 'The Time Stone', 'The Power Stone',
     'B', @Avengers);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @MissionImp)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who plays Ethan Hunt in Mission Impossible?',
     'General Movie Trivia',
     'Will Smith', 'Tom Cruise', 'Brad Pitt', 'Matt Damon',
     'B', @MissionImp),
    ('What is the name of the spy agency in Mission Impossible?',
     'General Movie Trivia',
     'CIA', 'MI6', 'IMF', 'NSA',
     'C', @MissionImp),
    ('In the original Mission Impossible what does Ethan Hunt famously steal?',
     'General Movie Trivia',
     'A nuclear weapon', 'The NOC list', 'A satellite code', 'A prototype jet',
     'B', @MissionImp);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @WolfWallSt)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who plays Jordan Belfort in The Wolf of Wall Street?',
     'General Movie Trivia',
     'Brad Pitt', 'Matt Damon', 'Leonardo DiCaprio', 'Christian Bale',
     'C', @WolfWallSt),
    ('Who directed The Wolf of Wall Street?',
     'General Movie Trivia',
     'Quentin Tarantino', 'Martin Scorsese', 'David Fincher', 'Ridley Scott',
     'B', @WolfWallSt),
    ('Jordan Belfort founded which brokerage firm in the film?',
     'General Movie Trivia',
     'Goldman Sachs', 'Lehman Brothers', 'Stratton Oakmont', 'Bear Stearns',
     'C', @WolfWallSt);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @PulpFiction)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who directed Pulp Fiction?',
     'General Movie Trivia',
     'Martin Scorsese', 'Quentin Tarantino', 'David Fincher', 'Spike Lee',
     'B', @PulpFiction),
    ('What is in the briefcase in Pulp Fiction?',
     'General Movie Trivia',
     'Gold bars', 'Diamonds', 'It is never revealed', 'Drug money',
     'C', @PulpFiction),
    ('Which actor plays Vincent Vega in Pulp Fiction?',
     'General Movie Trivia',
     'Bruce Willis', 'Samuel L. Jackson', 'John Travolta', 'Harvey Keitel',
     'C', @PulpFiction);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @Gladiator)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who plays Maximus in Gladiator?',
     'General Movie Trivia',
     'Mel Gibson', 'Russell Crowe', 'Brad Pitt', 'Antonio Banderas',
     'B', @Gladiator),
    ('Who directed Gladiator?',
     'General Movie Trivia',
     'James Cameron', 'Christopher Nolan', 'Ridley Scott', 'Steven Spielberg',
     'C', @Gladiator),
    ('What does Maximus say before fighting in the Colosseum?',
     'General Movie Trivia',
     'For the Republic', 'Are you not entertained', 'For Rome and glory', 'Death before dishonour',
     'B', @Gladiator);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions WHERE MovieId = @ForrestGump)
BEGIN
    INSERT INTO dbo.TriviaQuestions
        (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
    VALUES
    ('Who plays Forrest Gump?',
     'General Movie Trivia',
     'Tom Cruise', 'Kevin Costner', 'Tom Hanks', 'Denzel Washington',
     'C', @ForrestGump),
    ('What sport does Forrest excel at in college?',
     'General Movie Trivia',
     'Basketball', 'American Football', 'Baseball', 'Swimming',
     'B', @ForrestGump),
    ('What is Forrest''s famous quote about life?',
     'General Movie Trivia',
     'Life is what you make it',
     'Life is like a box of chocolates',
     'Every day is a gift',
     'Just keep running',
     'B', @ForrestGump);
END;
GO

-- =============================================
-- Marathon progress for dummy users
-- so leaderboards have real data
-- =============================================

DECLARE @NolanId INT          = (SELECT Id FROM dbo.Marathons WHERE Title = 'Nolan Week');
DECLARE @SciFiId INT          = (SELECT Id FROM dbo.Marathons WHERE Title = 'Sci-Fi Marathon');
DECLARE @ThrillerID INT       = (SELECT Id FROM dbo.Marathons WHERE Title = 'Thriller Night');
DECLARE @DramaId INT          = (SELECT Id FROM dbo.Marathons WHERE Title = 'Drama Classics');
DECLARE @ActionId INT         = (SELECT Id FROM dbo.Marathons WHERE Title = 'Action Legends');
DECLARE @DiCaprioId INT       = (SELECT Id FROM dbo.Marathons WHERE Title = 'DiCaprio Spotlight');

DECLARE @Alice INT   = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-alice');
DECLARE @Bob INT     = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-bob');
DECLARE @Carol INT   = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-carol');
DECLARE @Dan INT     = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-dan');
DECLARE @Eva INT     = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-eva');
DECLARE @Frank INT   = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-frank');
DECLARE @Grace INT   = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-grace');
DECLARE @Henry INT   = (SELECT Id FROM dbo.Users WHERE AuthSubject = 'user-henry');

-- We need to make sure these marathons are active and have a week scoping
-- so the progress rows reference valid active marathons.
-- The weekly assign logic will handle this on first load, but for the seed
-- we set them directly so progress can be inserted.

DECLARE @WeekString NVARCHAR(10) =
    CAST(YEAR(GETDATE()) AS NVARCHAR(4)) + '-W' +
    RIGHT('0' + CAST(DATEPART(ISO_WEEK, GETDATE()) AS NVARCHAR(2)), 2);

UPDATE dbo.Marathons SET IsActive = 1, WeekScoping = @WeekString
WHERE Title IN ('Nolan Week', 'Sci-Fi Marathon', 'Thriller Night',
                'Drama Classics', 'Action Legends', 'DiCaprio Spotlight');

-- ---- Nolan Week leaderboard ----

IF @Alice IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Alice AND MarathonId = @NolanId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Alice, @NolanId, DATEADD(hour, -5, GETDATE()), 100.0, 4,
            DATEADD(hour, -2, GETDATE()));

IF @Bob IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Bob AND MarathonId = @NolanId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Bob, @NolanId, DATEADD(hour, -6, GETDATE()), 100.0, 4,
            DATEADD(hour, -3, GETDATE()));

IF @Carol IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Carol AND MarathonId = @NolanId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Carol, @NolanId, DATEADD(hour, -4, GETDATE()), 66.7, 3, NULL);

IF @Dan IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Dan AND MarathonId = @NolanId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Dan, @NolanId, DATEADD(hour, -3, GETDATE()), 83.3, 2, NULL);

IF @Eva IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Eva AND MarathonId = @NolanId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Eva, @NolanId, DATEADD(hour, -2, GETDATE()), 50.0, 1, NULL);

IF @Frank IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Frank AND MarathonId = @NolanId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Frank, @NolanId, DATEADD(hour, -1, GETDATE()), 0.0, 0, NULL);

-- ---- Sci-Fi Marathon leaderboard ----

IF @Grace IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Grace AND MarathonId = @SciFiId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Grace, @SciFiId, DATEADD(hour, -8, GETDATE()), 100.0, 4,
            DATEADD(hour, -4, GETDATE()));

IF @Alice IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Alice AND MarathonId = @SciFiId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Alice, @SciFiId, DATEADD(hour, -7, GETDATE()), 83.3, 3, NULL);

IF @Henry IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Henry AND MarathonId = @SciFiId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Henry, @SciFiId, DATEADD(hour, -5, GETDATE()), 66.7, 2, NULL);

IF @Bob IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Bob AND MarathonId = @SciFiId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Bob, @SciFiId, DATEADD(hour, -3, GETDATE()), 33.3, 1, NULL);

-- ---- Thriller Night leaderboard ----

IF @Dan IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Dan AND MarathonId = @ThrillerID)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Dan, @ThrillerID, DATEADD(hour, -10, GETDATE()), 100.0, 3,
            DATEADD(hour, -6, GETDATE()));

IF @Eva IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Eva AND MarathonId = @ThrillerID)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Eva, @ThrillerID, DATEADD(hour, -9, GETDATE()), 100.0, 3,
            DATEADD(hour, -5, GETDATE()));

IF @Carol IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Carol AND MarathonId = @ThrillerID)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Carol, @ThrillerID, DATEADD(hour, -8, GETDATE()), 66.7, 2, NULL);

-- ---- Drama Classics leaderboard ----

IF @Henry IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Henry AND MarathonId = @DramaId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Henry, @DramaId, DATEADD(hour, -12, GETDATE()), 100.0, 3,
            DATEADD(hour, -8, GETDATE()));

IF @Grace IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Grace AND MarathonId = @DramaId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Grace, @DramaId, DATEADD(hour, -11, GETDATE()), 83.3, 2, NULL);

IF @Frank IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Frank AND MarathonId = @DramaId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Frank, @DramaId, DATEADD(hour, -10, GETDATE()), 33.3, 1, NULL);

-- ---- Action Legends leaderboard ----

IF @Bob IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Bob AND MarathonId = @ActionId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Bob, @ActionId, DATEADD(hour, -6, GETDATE()), 100.0, 3,
            DATEADD(hour, -3, GETDATE()));

IF @Alice IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Alice AND MarathonId = @ActionId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Alice, @ActionId, DATEADD(hour, -5, GETDATE()), 66.7, 2, NULL);

IF @Eva IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Eva AND MarathonId = @ActionId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Eva, @ActionId, DATEADD(hour, -4, GETDATE()), 33.3, 1, NULL);

-- ---- DiCaprio Spotlight leaderboard ----

IF @Carol IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Carol AND MarathonId = @DiCaprioId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Carol, @DiCaprioId, DATEADD(hour, -7, GETDATE()), 100.0, 3,
            DATEADD(hour, -4, GETDATE()));

IF @Dan IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Dan AND MarathonId = @DiCaprioId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Dan, @DiCaprioId, DATEADD(hour, -6, GETDATE()), 83.3, 2, NULL);

IF @Grace IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.MarathonProgress WHERE UserId = @Grace AND MarathonId = @DiCaprioId)
    INSERT INTO dbo.MarathonProgress
        (UserId, MarathonId, JoinedAt, TriviaAccuracy, CompletedMoviesCount, FinishedAt)
    VALUES (@Grace, @DiCaprioId, DATEADD(hour, -5, GETDATE()), 66.7, 1, NULL);
GO