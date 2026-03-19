USE [MovieApp];
GO

DECLARE @AuthProvider NVARCHAR(32) = N'dummy';
DECLARE @AuthSubject NVARCHAR(128) = N'default-user';

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Users
    WHERE AuthProvider = @AuthProvider
      AND AuthSubject = @AuthSubject
)
BEGIN
    INSERT INTO dbo.Users (AuthProvider, AuthSubject)
    VALUES (@AuthProvider, @AuthSubject);
END;
GO
