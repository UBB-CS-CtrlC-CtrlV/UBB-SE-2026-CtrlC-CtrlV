USE [MovieApp];
GO

DECLARE @AuthProvider NVARCHAR(32) = N'dummy';
DECLARE @AuthSubject NVARCHAR(128) = N'default-user';
DECLARE @Username NVARCHAR(64) = N'Dummy User';

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Users
    WHERE AuthProvider = @AuthProvider
      AND AuthSubject = @AuthSubject
)
BEGIN
    INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username)
    VALUES (@AuthProvider, @AuthSubject, @Username);
END;
ELSE
BEGIN
    UPDATE dbo.Users
    SET Username = @Username
    WHERE AuthProvider = @AuthProvider
      AND AuthSubject = @AuthSubject;
END;
GO
