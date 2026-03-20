USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Movies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Movies
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ReleaseYear INT NULL,
        DurationMinutes INT NULL,

        CONSTRAINT PK_Movies PRIMARY KEY (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Genres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Genres
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Genres PRIMARY KEY (Id),
        CONSTRAINT UQ_Genres_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.MovieGenres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MovieGenres
    (
        MovieId INT NOT NULL,
        GenreId INT NOT NULL,

        CONSTRAINT PK_MovieGenres PRIMARY KEY (MovieId, GenreId),

        CONSTRAINT FK_MovieGenres_Movies
            FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),

        CONSTRAINT FK_MovieGenres_Genres
            FOREIGN KEY (GenreId) REFERENCES dbo.Genres(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Actors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Actors
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(200) NOT NULL,

        CONSTRAINT PK_Actors PRIMARY KEY (Id),
        CONSTRAINT UQ_Actors_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.MovieActors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MovieActors
    (
        MovieId INT NOT NULL,
        ActorId INT NOT NULL,

        CONSTRAINT PK_MovieActors PRIMARY KEY (MovieId, ActorId),

        CONSTRAINT FK_MovieActors_Movies
            FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),

        CONSTRAINT FK_MovieActors_Actors
            FOREIGN KEY (ActorId) REFERENCES dbo.Actors(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Directors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Directors
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(200) NOT NULL,

        CONSTRAINT PK_Directors PRIMARY KEY (Id),
        CONSTRAINT UQ_Directors_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.MovieDirectors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MovieDirectors
    (
        MovieId INT NOT NULL,
        DirectorId INT NOT NULL,

        CONSTRAINT PK_MovieDirectors PRIMARY KEY (MovieId, DirectorId),

        CONSTRAINT FK_MovieDirectors_Movies
            FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),

        CONSTRAINT FK_MovieDirectors_Directors
            FOREIGN KEY (DirectorId) REFERENCES dbo.Directors(Id)
    );
END;
GO