IF NOT EXISTS (SELECT 1 FROM NotificationPreference
               WHERE UserId = (SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'))
BEGIN
    INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
    VALUES
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Payment', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Inbound Transfer', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Outbound Transfer', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Low Balance', 1, 0, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Due Payment', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Suspicious Activity', 1, 1, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM NotificationPreference
               WHERE UserId = (SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'))
BEGIN
    INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
    VALUES
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Payment', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Inbound Transfer', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Outbound Transfer', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Low Balance', 1, 0, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Due Payment', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Suspicious Activity', 1, 1, 1);
END
GO
