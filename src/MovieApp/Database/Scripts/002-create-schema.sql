USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        AuthProvider NVARCHAR(32) NOT NULL,
        AuthSubject NVARCHAR(128) NOT NULL,
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Users_AuthProvider_AuthSubject UNIQUE (AuthProvider, AuthSubject)
    );
END;
GO
