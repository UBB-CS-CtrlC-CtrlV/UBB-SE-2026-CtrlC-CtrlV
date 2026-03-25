USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Screenings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Screenings
    (
        EventId INT NOT NULL,
        MovieId INT NOT NULL,
        CONSTRAINT PK_Screenings PRIMARY KEY (EventId, MovieId),
        CONSTRAINT FK_Screenings_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id),
        CONSTRAINT FK_Screenings_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id)
    );
END;
GO
