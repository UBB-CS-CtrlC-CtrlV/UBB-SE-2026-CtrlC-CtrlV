USE [MovieApp];
GO

-- Seed Genres
IF NOT EXISTS (SELECT 1 FROM dbo.Genres WHERE Name = 'Action')
BEGIN
    INSERT INTO dbo.Genres (Name) VALUES 
    ('Action'),
    ('Comedy'),
    ('Drama'),
    ('Horror'),
    ('Romance'),
    ('Science Fiction'),
    ('Thriller'),
    ('Animation'),
    ('Adventure'),
    ('Mystery');
END;
GO

-- Seed Actors
IF NOT EXISTS (SELECT 1 FROM dbo.Actors WHERE Name = 'Tom Cruise')
BEGIN
    INSERT INTO dbo.Actors (Name) VALUES 
    ('Tom Cruise'),
    ('Scarlett Johansson'),
    ('Leonardo DiCaprio'),
    ('Emma Watson'),
    ('Dwayne Johnson'),
    ('Jennifer Lawrence'),
    ('Tom Hanks'),
    ('Angelina Jolie'),
    ('Chris Evans'),
    ('Natalie Portman'),
    ('Johnny Depp'),
    ('Meryl Streep');
END;
GO

-- Seed Directors
IF NOT EXISTS (SELECT 1 FROM dbo.Directors WHERE Name = 'Steven Spielberg')
BEGIN
    INSERT INTO dbo.Directors (Name) VALUES 
    ('Steven Spielberg'),
    ('Christopher Nolan'),
    ('Martin Scorsese'),
    ('Quentin Tarantino'),
    ('Ridley Scott'),
    ('James Cameron'),
    ('Spike Lee'),
    ('David Fincher'),
    ('Denis Villeneuve'),
    ('Wes Anderson'),
    ('Ari Aster'),
    ('Paul Thomas Anderson');
END;
GO

-- Insert Movies if they don't exist
IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE Title = 'Mission Impossible')
BEGIN
    INSERT INTO dbo.Movies (Title, Description, ReleaseYear, DurationMinutes) VALUES 
    ('Mission Impossible', 'A spy action-thriller series following Ethan Hunt', 1996, 110),
    ('The Avengers', 'Marvel superhero team assembles', 2012, 143),
    ('Inception', 'A mind-bending science fiction thriller', 2010, 148),
    ('The Wolf of Wall Street', 'A dramatic biography of a con artist', 2013, 180),
    ('Pulp Fiction', 'An interwoven crime thriller', 1994, 154),
    ('Interstellar', 'A epic science fiction adventure', 2014, 169),
    ('Gladiator', 'A historical action epic', 2000, 155),
    ('The Dark Knight', 'A crime thriller about Batman', 2008, 152),
    ('Forrest Gump', 'A drama about a mans life journey', 1994, 142),
    ('Avatar', 'A science fiction epic on an alien world', 2009, 162),
    ('Blade Runner 2049', 'A science fiction noir thriller', 2017, 164),
    ('Dunkirk', 'A war drama thriller', 2017, 106);
END;
GO

-- Link movies with genres (all in one batch to avoid variable scope issues)
DECLARE @MovieId INT, @ActionId INT, @DramaId INT, @SciFiId INT, @ThrillerID INT;

SELECT @ActionId = Id FROM dbo.Genres WHERE Name = 'Action';
SELECT @DramaId = Id FROM dbo.Genres WHERE Name = 'Drama';
SELECT @SciFiId = Id FROM dbo.Genres WHERE Name = 'Science Fiction';
SELECT @ThrillerID = Id FROM dbo.Genres WHERE Name = 'Thriller';

-- Movie 1: Mission Impossible
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Mission Impossible';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ActionId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ActionId);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ThrillerID)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ThrillerID);

-- Movie 2: The Avengers
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Avengers';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ActionId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ActionId);

-- Movie 3: Inception
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Inception';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @SciFiId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @SciFiId);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ThrillerID)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ThrillerID);

-- Movie 4: The Wolf of Wall Street
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Wolf of Wall Street';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @DramaId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @DramaId);

-- Movie 5: Pulp Fiction
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Pulp Fiction';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ThrillerID)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ThrillerID);

-- Movie 6: Interstellar
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Interstellar';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @SciFiId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @SciFiId);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @DramaId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @DramaId);

-- Movie 7: Gladiator
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Gladiator';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ActionId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ActionId);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @DramaId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @DramaId);

-- Movie 8: The Dark Knight
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Dark Knight';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ThrillerID)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ThrillerID);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ActionId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ActionId);

-- Movie 9: Forrest Gump
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Forrest Gump';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @DramaId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @DramaId);

-- Movie 10: Avatar
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Avatar';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @SciFiId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @SciFiId);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ActionId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ActionId);

-- Movie 11: Blade Runner 2049
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Blade Runner 2049';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @SciFiId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @SciFiId);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ThrillerID)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ThrillerID);

-- Movie 12: Dunkirk
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Dunkirk';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @ThrillerID)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @ThrillerID);
IF NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieId AND GenreId = @DramaId)
    INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@MovieId, @DramaId);
GO

-- Link movies with actors (all in one batch)
DECLARE @MovieId INT, @TomCruiseId INT, @Scarlett INT, @DiCaprio INT, @EmmaWatson INT, @DwayneJohnson INT;

SELECT @TomCruiseId = Id FROM dbo.Actors WHERE Name = 'Tom Cruise';
SELECT @Scarlett = Id FROM dbo.Actors WHERE Name = 'Scarlett Johansson';
SELECT @DiCaprio = Id FROM dbo.Actors WHERE Name = 'Leonardo DiCaprio';
SELECT @EmmaWatson = Id FROM dbo.Actors WHERE Name = 'Emma Watson';
SELECT @DwayneJohnson = Id FROM dbo.Actors WHERE Name = 'Dwayne Johnson';

-- Movie 1: Mission Impossible - Tom Cruise
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Mission Impossible';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @TomCruiseId)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @TomCruiseId);

-- Movie 2: The Avengers - Scarlett
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Avengers';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @Scarlett)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @Scarlett);

-- Movie 3: Inception - DiCaprio
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Inception';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @DiCaprio)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @DiCaprio);

-- Movie 4: The Wolf of Wall Street - DiCaprio
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Wolf of Wall Street';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @DiCaprio)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @DiCaprio);

-- Movie 5: Pulp Fiction - Tom Cruise
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Pulp Fiction';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @TomCruiseId)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @TomCruiseId);

-- Movie 6: Interstellar - DiCaprio
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Interstellar';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @DiCaprio)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @DiCaprio);

-- Movie 7: Gladiator - Dwayne Johnson
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Gladiator';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @DwayneJohnson)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @DwayneJohnson);

-- Movie 8: The Dark Knight - Scarlett
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Dark Knight';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @Scarlett)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @Scarlett);

-- Movie 9: Forrest Gump - Emma Watson
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Forrest Gump';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @EmmaWatson)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @EmmaWatson);

-- Movie 10: Avatar - Dwayne Johnson
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Avatar';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @DwayneJohnson)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @DwayneJohnson);

-- Movie 11: Blade Runner 2049 - Emma Watson
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Blade Runner 2049';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @EmmaWatson)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @EmmaWatson);

-- Movie 12: Dunkirk - Tom Cruise
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Dunkirk';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieActors WHERE MovieId = @MovieId AND ActorId = @TomCruiseId)
    INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@MovieId, @TomCruiseId);
GO

-- Link movies with directors (all in one batch)
DECLARE @MovieId INT, @SpielbergId INT, @NolanId INT, @ScorseseId INT, @TarantinoId INT, @RidleyId INT;

SELECT @SpielbergId = Id FROM dbo.Directors WHERE Name = 'Steven Spielberg';
SELECT @NolanId = Id FROM dbo.Directors WHERE Name = 'Christopher Nolan';
SELECT @ScorseseId = Id FROM dbo.Directors WHERE Name = 'Martin Scorsese';
SELECT @TarantinoId = Id FROM dbo.Directors WHERE Name = 'Quentin Tarantino';
SELECT @RidleyId = Id FROM dbo.Directors WHERE Name = 'Ridley Scott';

-- Movie 1: Mission Impossible - Spielberg
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Mission Impossible';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @SpielbergId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @SpielbergId);

-- Movie 2: The Avengers - Nolan
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Avengers';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @NolanId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @NolanId);

-- Movie 3: Inception - Nolan
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Inception';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @NolanId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @NolanId);

-- Movie 4: The Wolf of Wall Street - Scorsese
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Wolf of Wall Street';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @ScorseseId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @ScorseseId);

-- Movie 5: Pulp Fiction - Tarantino
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Pulp Fiction';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @TarantinoId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @TarantinoId);

-- Movie 6: Interstellar - Nolan
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Interstellar';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @NolanId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @NolanId);

-- Movie 7: Gladiator - Ridley
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Gladiator';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @RidleyId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @RidleyId);

-- Movie 8: The Dark Knight - Nolan
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'The Dark Knight';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @NolanId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @NolanId);

-- Movie 9: Forrest Gump - Spielberg
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Forrest Gump';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @SpielbergId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @SpielbergId);

-- Movie 10: Avatar - Tarantino
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Avatar';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @TarantinoId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @TarantinoId);

-- Movie 11: Blade Runner 2049 - Ridley
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Blade Runner 2049';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @RidleyId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @RidleyId);

-- Movie 12: Dunkirk - Nolan
SELECT @MovieId = Id FROM dbo.Movies WHERE Title = 'Dunkirk';
IF NOT EXISTS (SELECT 1 FROM dbo.MovieDirectors WHERE MovieId = @MovieId AND DirectorId = @NolanId)
    INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@MovieId, @NolanId);
GO
