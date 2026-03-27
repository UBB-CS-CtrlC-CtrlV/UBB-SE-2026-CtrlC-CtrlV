USE [MovieApp];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = 'UserEventAttendance' AND SCHEMA_NAME(schema_id) = 'dbo'
)
BEGIN
    CREATE TABLE dbo.UserEventAttendance (
        Id       INT           IDENTITY(1,1) NOT NULL
                                   CONSTRAINT PK_UserEventAttendance PRIMARY KEY,
        UserId   INT           NOT NULL,
        EventId  INT           NOT NULL,
        JoinedAt DATETIME2     NOT NULL
                                   CONSTRAINT DF_UserEventAttendance_JoinedAt DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_UserEventAttendance_UserEvent UNIQUE (UserId, EventId)
    );
END;
GO
