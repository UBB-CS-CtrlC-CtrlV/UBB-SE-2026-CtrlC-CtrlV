USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.UserMovieDiscounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserMovieDiscounts
    (
        Id INT IDENTITY(1,1) NOT NULL,

        UserId INT NOT NULL,
        MovieId INT NOT NULL,

        DiscountPercentage DECIMAL(5,2) NOT NULL,

        CreatedAt DATETIME NOT NULL 
            CONSTRAINT DF_UserMovieDiscounts_CreatedAt DEFAULT (GETDATE()),

        CONSTRAINT PK_UserMovieDiscounts PRIMARY KEY (Id),

        CONSTRAINT FK_UserMovieDiscounts_Users
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),

        CONSTRAINT FK_UserMovieDiscounts_Movies
            FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),

        CONSTRAINT CK_UserMovieDiscounts_Percentage
            CHECK (DiscountPercentage >= 0 AND DiscountPercentage <= 100)
    );
END;
GO