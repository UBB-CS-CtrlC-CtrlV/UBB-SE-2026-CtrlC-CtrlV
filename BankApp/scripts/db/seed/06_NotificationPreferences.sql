IF NOT EXISTS (SELECT 1 FROM NotificationPreference
               WHERE UserId = (SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'))
BEGIN
    INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
    VALUES
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Transactions', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Security', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'Promotions', 0, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'), 'AccountUpdates', 1, 1, 0);
END
GO

IF NOT EXISTS (SELECT 1 FROM NotificationPreference
               WHERE UserId = (SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'))
BEGIN
    INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
    VALUES
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Transactions', 1, 1, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Security', 1, 1, 1),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'Promotions', 0, 0, 0),
    ((SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'), 'AccountUpdates', 1, 0, 0);
END
GO