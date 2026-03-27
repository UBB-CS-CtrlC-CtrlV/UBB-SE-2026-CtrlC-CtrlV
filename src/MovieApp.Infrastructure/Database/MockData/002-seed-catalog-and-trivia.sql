USE [MovieApp];
GO

-- Extends the movie catalog and attaches movie-specific trivia so both the
-- slot-machine and marathon flows have enough linked reference data.

;WITH GenreSeeds(Name) AS
(
    SELECT N'Biography' UNION ALL
    SELECT N'Fantasy'
)
INSERT INTO dbo.Genres (Name)
SELECT seed.Name
FROM GenreSeeds seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Genres existing
    WHERE existing.Name = seed.Name
);
GO

;WITH ActorSeeds(Name) AS
(
    SELECT N'Ryan Gosling' UNION ALL
    SELECT N'Amy Adams' UNION ALL
    SELECT N'Margot Robbie' UNION ALL
    SELECT N'Matt Damon'
)
INSERT INTO dbo.Actors (Name)
SELECT seed.Name
FROM ActorSeeds seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Actors existing
    WHERE existing.Name = seed.Name
);
GO

;WITH DirectorSeeds(Name) AS
(
    SELECT N'Damien Chazelle' UNION ALL
    SELECT N'Greta Gerwig' UNION ALL
    SELECT N'George Miller'
)
INSERT INTO dbo.Directors (Name)
SELECT seed.Name
FROM DirectorSeeds seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Directors existing
    WHERE existing.Name = seed.Name
);
GO

;WITH MovieSeeds(Title, Description, ReleaseYear, DurationMinutes) AS
(
    SELECT N'La La Land', N'A musical romance about ambition, compromise, and timing.', 2016, 128 UNION ALL
    SELECT N'Arrival', N'A science-fiction drama about language, time, and first contact.', 2016, 116 UNION ALL
    SELECT N'Mad Max: Fury Road', N'A relentless chase across a post-apocalyptic wasteland.', 2015, 120 UNION ALL
    SELECT N'Barbie', N'A candy-colored satire about identity, expectation, and self-discovery.', 2023, 114
)
INSERT INTO dbo.Movies (Title, Description, ReleaseYear, DurationMinutes)
SELECT seed.Title, seed.Description, seed.ReleaseYear, seed.DurationMinutes
FROM MovieSeeds seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Movies existing
    WHERE existing.Title = seed.Title
);
GO

;WITH MovieGenreSeeds(MovieTitle, GenreName) AS
(
    SELECT N'La La Land', N'Romance' UNION ALL
    SELECT N'La La Land', N'Drama' UNION ALL
    SELECT N'Arrival', N'Science Fiction' UNION ALL
    SELECT N'Arrival', N'Drama' UNION ALL
    SELECT N'Mad Max: Fury Road', N'Action' UNION ALL
    SELECT N'Mad Max: Fury Road', N'Adventure' UNION ALL
    SELECT N'Barbie', N'Comedy' UNION ALL
    SELECT N'Barbie', N'Adventure'
)
INSERT INTO dbo.MovieGenres (MovieId, GenreId)
SELECT movie.Id, genre.Id
FROM MovieGenreSeeds seed
INNER JOIN dbo.Movies movie ON movie.Title = seed.MovieTitle
INNER JOIN dbo.Genres genre ON genre.Name = seed.GenreName
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.MovieGenres existing
    WHERE existing.MovieId = movie.Id
      AND existing.GenreId = genre.Id
);
GO

;WITH MovieActorSeeds(MovieTitle, ActorName) AS
(
    SELECT N'La La Land', N'Ryan Gosling' UNION ALL
    SELECT N'Arrival', N'Amy Adams' UNION ALL
    SELECT N'Mad Max: Fury Road', N'Matt Damon' UNION ALL
    SELECT N'Barbie', N'Margot Robbie'
)
INSERT INTO dbo.MovieActors (MovieId, ActorId)
SELECT movie.Id, actor.Id
FROM MovieActorSeeds seed
INNER JOIN dbo.Movies movie ON movie.Title = seed.MovieTitle
INNER JOIN dbo.Actors actor ON actor.Name = seed.ActorName
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.MovieActors existing
    WHERE existing.MovieId = movie.Id
      AND existing.ActorId = actor.Id
);
GO

;WITH MovieDirectorSeeds(MovieTitle, DirectorName) AS
(
    SELECT N'La La Land', N'Damien Chazelle' UNION ALL
    SELECT N'Arrival', N'Denis Villeneuve' UNION ALL
    SELECT N'Mad Max: Fury Road', N'George Miller' UNION ALL
    SELECT N'Barbie', N'Greta Gerwig'
)
INSERT INTO dbo.MovieDirectors (MovieId, DirectorId)
SELECT movie.Id, director.Id
FROM MovieDirectorSeeds seed
INNER JOIN dbo.Movies movie ON movie.Title = seed.MovieTitle
INNER JOIN dbo.Directors director ON director.Name = seed.DirectorName
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.MovieDirectors existing
    WHERE existing.MovieId = movie.Id
      AND existing.DirectorId = director.Id
);
GO

;WITH TriviaSeeds(MovieTitle, Category, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption) AS
(
    SELECT N'Inception', N'General Movie Trivia', N'Who directed Inception?', N'Christopher Nolan', N'Denis Villeneuve', N'Ridley Scott', N'James Cameron', 'A' UNION ALL
    SELECT N'Inception', N'General Movie Trivia', N'What is Dom Cobb hired to perform?', N'An extraction', N'An inception', N'A heist', N'A surveillance mission', 'B' UNION ALL
    SELECT N'Inception', N'General Movie Trivia', N'Which city folds over itself during the training sequence?', N'Paris', N'London', N'Tokyo', N'Chicago', 'A' UNION ALL

    SELECT N'Interstellar', N'General Movie Trivia', N'What is the name of Matthew McConaughey''s character in Interstellar?', N'Cooper', N'Murph', N'TARS', N'Doyle', 'A' UNION ALL
    SELECT N'Interstellar', N'General Movie Trivia', N'Which robot accompanies the crew in Interstellar?', N'EDITH', N'TARS', N'BAYMAX', N'WALL-E', 'B' UNION ALL
    SELECT N'Interstellar', N'General Movie Trivia', N'What celestial phenomenon do the astronauts travel through?', N'A black hole named Gargantua', N'A comet named Halley', N'A wormhole near Saturn', N'A nebula in Orion', 'C' UNION ALL

    SELECT N'The Dark Knight', N'General Movie Trivia', N'Who played the Joker in The Dark Knight?', N'Jared Leto', N'Heath Ledger', N'Joaquin Phoenix', N'Jack Nicholson', 'B' UNION ALL
    SELECT N'The Dark Knight', N'General Movie Trivia', N'What is Harvey Dent''s public role before becoming Two-Face?', N'Police commissioner', N'District attorney', N'Mayor', N'Judge', 'B' UNION ALL
    SELECT N'The Dark Knight', N'General Movie Trivia', N'Which Gotham vigilante is central to the film?', N'Superman', N'Spider-Man', N'Batman', N'The Flash', 'C' UNION ALL

    SELECT N'Dunkirk', N'General Movie Trivia', N'Dunkirk dramatizes the evacuation of allied troops from which location?', N'Normandy', N'Dunkirk', N'Sicily', N'Calais', 'B' UNION ALL
    SELECT N'Dunkirk', N'General Movie Trivia', N'Who directed Dunkirk?', N'Christopher Nolan', N'Sam Mendes', N'David Lean', N'Ridley Scott', 'A' UNION ALL
    SELECT N'Dunkirk', N'General Movie Trivia', N'Which branch is represented by the small civilian boats in Dunkirk?', N'The merchant navy', N'The home fleet', N'The civilian rescue effort', N'The coast guard reserve', 'C' UNION ALL

    SELECT N'Avatar', N'General Movie Trivia', N'What is the name of the moon where Avatar takes place?', N'Titan', N'Pandora', N'Europa', N'Krypton', 'B' UNION ALL
    SELECT N'Avatar', N'General Movie Trivia', N'Who directed Avatar?', N'James Cameron', N'Peter Jackson', N'Christopher Nolan', N'George Miller', 'A' UNION ALL
    SELECT N'Avatar', N'General Movie Trivia', N'What are the blue humanoid inhabitants of Pandora called?', N'Kryptonians', N'Na''vi', N'Asari', N'Synths', 'B' UNION ALL

    SELECT N'Blade Runner 2049', N'General Movie Trivia', N'What is the name of Ryan Gosling''s blade runner in Blade Runner 2049?', N'K', N'Roy', N'Deckard', N'Sapper', 'A' UNION ALL
    SELECT N'Blade Runner 2049', N'General Movie Trivia', N'Who directed Blade Runner 2049?', N'Ridley Scott', N'Denis Villeneuve', N'Alex Garland', N'David Fincher', 'B' UNION ALL
    SELECT N'Blade Runner 2049', N'General Movie Trivia', N'What is the name of K''s holographic companion?', N'Rachael', N'Joi', N'Luv', N'Pris', 'B' UNION ALL

    SELECT N'The Avengers', N'General Movie Trivia', N'Which organization assembles the Avengers?', N'S.H.I.E.L.D.', N'Hydra', N'Star Labs', N'Weapon X', 'A' UNION ALL
    SELECT N'The Avengers', N'General Movie Trivia', N'Which villain leads the invasion in The Avengers?', N'Thanos', N'Ultron', N'Loki', N'Red Skull', 'C' UNION ALL
    SELECT N'The Avengers', N'General Movie Trivia', N'Which city faces the climactic alien attack?', N'Los Angeles', N'New York', N'Chicago', N'Washington', 'B' UNION ALL

    SELECT N'Mission Impossible', N'General Movie Trivia', N'Who is the central IMF agent in Mission Impossible?', N'Jason Bourne', N'Ethan Hunt', N'Jack Ryan', N'John Wick', 'B' UNION ALL
    SELECT N'Mission Impossible', N'General Movie Trivia', N'What does IMF stand for in Mission Impossible?', N'Impossible Missions Force', N'International Mission Federation', N'Internal Military Front', N'Impossible Mobility Force', 'A' UNION ALL
    SELECT N'Mission Impossible', N'General Movie Trivia', N'Which actor is most associated with Ethan Hunt?', N'Matt Damon', N'Tom Cruise', N'Keanu Reeves', N'Brad Pitt', 'B' UNION ALL

    SELECT N'Gladiator', N'General Movie Trivia', N'What is Maximus''s rank at the start of Gladiator?', N'General', N'Centurion', N'Senator', N'Emperor', 'A' UNION ALL
    SELECT N'Gladiator', N'General Movie Trivia', N'Who directed Gladiator?', N'Ridley Scott', N'Oliver Stone', N'Wolfgang Petersen', N'James Cameron', 'A' UNION ALL
    SELECT N'Gladiator', N'General Movie Trivia', N'Which emperor betrays Maximus and seizes power?', N'Marcus Aurelius', N'Commodus', N'Nero', N'Augustus', 'B' UNION ALL

    SELECT N'Arrival', N'General Movie Trivia', N'What is Louise Banks''s profession in Arrival?', N'Physicist', N'Linguist', N'Pilot', N'Journalist', 'B' UNION ALL
    SELECT N'Arrival', N'General Movie Trivia', N'Who directed Arrival?', N'Damien Chazelle', N'Denis Villeneuve', N'Greta Gerwig', N'Darren Aronofsky', 'B' UNION ALL
    SELECT N'Arrival', N'General Movie Trivia', N'Arrival centers on humanity communicating with what?', N'Robots', N'An AI network', N'Extraterrestrials', N'Time travelers', 'C' UNION ALL

    SELECT N'La La Land', N'General Movie Trivia', N'What are the names of the two leads in La La Land?', N'Sebastian and Mia', N'Jack and Rose', N'Noah and Allie', N'Romeo and Juliet', 'A' UNION ALL
    SELECT N'La La Land', N'General Movie Trivia', N'Who directed La La Land?', N'Damien Chazelle', N'Baz Luhrmann', N'Greta Gerwig', N'Rob Marshall', 'A' UNION ALL
    SELECT N'La La Land', N'General Movie Trivia', N'Which city is central to La La Land?', N'New York', N'Paris', N'Los Angeles', N'Chicago', 'C'
)
INSERT INTO dbo.TriviaQuestions
(
    QuestionText,
    Category,
    OptionA,
    OptionB,
    OptionC,
    OptionD,
    CorrectOption,
    MovieId
)
SELECT
    seed.QuestionText,
    seed.Category,
    seed.OptionA,
    seed.OptionB,
    seed.OptionC,
    seed.OptionD,
    seed.CorrectOption,
    movie.Id
FROM TriviaSeeds seed
INNER JOIN dbo.Movies movie ON movie.Title = seed.MovieTitle
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.TriviaQuestions existing
    WHERE existing.QuestionText = seed.QuestionText
);
GO
