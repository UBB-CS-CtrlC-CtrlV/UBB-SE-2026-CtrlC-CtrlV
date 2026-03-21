USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Events
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        Title NVARCHAR(200) NOT NULL,                
        Description NVARCHAR(MAX) NULL,               
        PosterUrl NVARCHAR(500) NOT NULL 
            CONSTRAINT DF_Events_Poster DEFAULT '',    
        EventDateTime DATETIME2 NOT NULL,              
        LocationReference NVARCHAR(255) NOT NULL,      
        TicketPrice DECIMAL(18, 2) NOT NULL,           
        
        HistoricalRating FLOAT NOT NULL 
            CONSTRAINT DF_Events_Rating DEFAULT 0.0,            
        MaxCapacity INT NOT NULL 
            CONSTRAINT DF_Events_Capacity DEFAULT 50,
        CurrentEnrollment INT NOT NULL 
            CONSTRAINT DF_Events_Enrollment DEFAULT 0,
        
        CreatorUserId INT NOT NULL,              

        CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Events_Users FOREIGN KEY (CreatorUserId) 
            REFERENCES dbo.Users(Id),

        CONSTRAINT CK_Events_TicketPrice CHECK (TicketPrice >= 0),

        CONSTRAINT CK_Events_HistoricalRating CHECK (HistoricalRating >= 0 AND HistoricalRating <= 5),

        CONSTRAINT CK_Events_MaxCapacity CHECK (MaxCapacity > 0),

        CONSTRAINT CK_Events_CurrentEnrollment CHECK (CurrentEnrollment >= 0 AND CurrentEnrollment <= MaxCapacity)
    );
END;
GO