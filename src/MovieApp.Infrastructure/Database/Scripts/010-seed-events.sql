USE [MovieApp];
GO

-- Assume User ID 1 exists from 003-seed-dummy-user.sql
DECLARE @CreatorId INT = (SELECT TOP 1 Id FROM dbo.Users ORDER BY Id);

IF @CreatorId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Events WHERE Title = 'Cannes Winner Screening')
    BEGIN
        INSERT INTO dbo.Events (Title, Description, PosterUrl, EventDateTime, LocationReference, TicketPrice, EventType, HistoricalRating, MaxCapacity, CurrentEnrollment, CreatorUserId)
        VALUES
        ('Cannes Winner Screening', 'Special screening of the Palme d''Or winner.', '', DATEADD(day, 2, GETDATE()), 'Cinema Hall A', 25.00, 'Premiere', 4.8, 100, 45, @CreatorId),
        ('Vintage Film Marathon', 'Back-to-back classics from the 50s.', '', DATEADD(day, 5, GETDATE()), 'Retro Cinema', 40.00, 'Marathon', 4.5, 50, 10, @CreatorId),
        ('Director''s Q&A: Sci-Fi Night', 'Watch the latest sci-fi hit followed by a talk.', '', DATEADD(day, 7, GETDATE()), 'Tech Hub Theater', 15.00, 'Special', 4.9, 200, 150, @CreatorId),
        ('Indie Documentary Showcase', 'Exploring hidden stories from across the globe.', '', DATEADD(day, -1, GETDATE()), 'Art House', 10.00, 'Screening', 4.2, 30, 30, @CreatorId);
    END
END
GO
