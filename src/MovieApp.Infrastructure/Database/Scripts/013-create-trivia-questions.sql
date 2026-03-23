USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.TriviaQuestions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TriviaQuestions
    (
        Id INT IDENTITY(1,1) NOT NULL,
        QuestionText NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(64) NOT NULL,
        OptionA NVARCHAR(256) NOT NULL,
        OptionB NVARCHAR(256) NOT NULL,
        OptionC NVARCHAR(256) NOT NULL,
        OptionD NVARCHAR(256) NOT NULL,
        CorrectOption CHAR(1) NOT NULL,
        MovieId INT NULL,

        CONSTRAINT PK_TriviaQuestions PRIMARY KEY (Id),

        CONSTRAINT FK_TriviaQuestions_Movies
            FOREIGN KEY (MovieId)
            REFERENCES dbo.Movies(Id),

        CONSTRAINT CK_TriviaQuestions_CorrectOption
            CHECK (CorrectOption IN ('A', 'B', 'C', 'D'))
    );
END;
GO