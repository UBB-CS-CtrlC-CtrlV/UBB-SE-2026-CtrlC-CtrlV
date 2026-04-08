IF NOT EXISTS (SELECT 1 FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000')
BEGIN
    INSERT INTO Account (UserId, AccountName, IBAN, Currency, Balance, AccountType, Status)
    VALUES (
        (SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'),
        'John''s Current Account',
        'RO49AAAA1B31007593840000',
        'RON',
        12500.00,
        'Current',
        'Active'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Account WHERE IBAN = 'RO49AAAA1B31007593840001')
BEGIN
    INSERT INTO Account (UserId, AccountName, IBAN, Currency, Balance, AccountType, Status)
    VALUES (
        (SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'),
        'John''s Savings Account',
        'RO49AAAA1B31007593840001',
        'RON',
        45000.00,
        'Savings',
        'Active'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Account WHERE IBAN = 'RO49AAAA1B31007593840002')
BEGIN
    INSERT INTO Account (UserId, AccountName, IBAN, Currency, Balance, AccountType, Status)
    VALUES (
        (SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'),
        'Jane''s Current Account',
        'RO49AAAA1B31007593840002',
        'RON',
        8750.50,
        'Current',
        'Active'
    );
END
GO