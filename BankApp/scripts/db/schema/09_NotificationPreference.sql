IF OBJECT_ID('dbo.NotificationPreference', 'U') IS NULL
CREATE TABLE NotificationPreference (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Category VARCHAR(30) NOT NULL,
    PushEnabled BIT DEFAULT 1,
    EmailEnabled BIT DEFAULT 1,
    SmsEnabled BIT DEFAULT 0,
    MinAmountThreshold DECIMAL(18,2),
    CONSTRAINT CK_NotificationPreference_Category CHECK (Category IN (
        'Payment',
        'Inbound Transfer',
        'Outbound Transfer',
        'Low Balance',
        'Due Payment',
        'Suspicious Activity'
    ))
);
GO