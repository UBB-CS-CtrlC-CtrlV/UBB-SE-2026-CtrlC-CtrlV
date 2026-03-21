USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Marathons', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Marathons
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        PosterUrl NVARCHAR(500) NOT NULL CONSTRAINT DF_Marathons_Poster DEFAULT '',
        Theme NVARCHAR(100) NULL,
        
        PrerequisiteMarathonId INT NULL,
        
        IsActive BIT NOT NULL CONSTRAINT DF_Marathons_Active DEFAULT 0,
        LastFeaturedDate DATETIME2 NULL,
        WeekScoping NVARCHAR(50) NULL, -- e.g., '2026-W12'

        CONSTRAINT PK_Marathons PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Marathons_Prereq FOREIGN KEY (PrerequisiteMarathonId) 
            REFERENCES dbo.Marathons(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.MarathonMovies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MarathonMovies
    (
        MarathonId INT NOT NULL,
        MovieId INT NOT NULL,

        CONSTRAINT PK_MarathonMovies PRIMARY KEY (MarathonId, MovieId),
        CONSTRAINT FK_MarathonMovies_Marathon FOREIGN KEY (MarathonId) 
            REFERENCES dbo.Marathons(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MarathonMovies_Movie FOREIGN KEY (MovieId) 
            REFERENCES dbo.Movies(Id) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.MarathonProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MarathonProgress
    (
        UserId INT NOT NULL,
        MarathonId INT NOT NULL,
        
        JoinedAt DATETIME2 NOT NULL CONSTRAINT DF_Progress_Joined DEFAULT GETDATE(),

        TriviaAccuracy FLOAT NOT NULL CONSTRAINT DF_Progress_Accuracy DEFAULT 0.0,
        CompletedMoviesCount INT NOT NULL CONSTRAINT DF_Progress_Count DEFAULT 0,
        
        FinishedAt DATETIME2 NULL, 

        CONSTRAINT PK_MarathonProgress PRIMARY KEY (UserId, MarathonId),
        CONSTRAINT FK_MarathonProgress_Users FOREIGN KEY (UserId) 
            REFERENCES dbo.Users(Id),
        CONSTRAINT FK_MarathonProgress_Marathons FOREIGN KEY (MarathonId) 
            REFERENCES dbo.Marathons(Id),
        
        CONSTRAINT CK_Progress_Accuracy CHECK (TriviaAccuracy >= 0 AND TriviaAccuracy <= 100)
    );
END;
GO