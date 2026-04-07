USE BankAppDb;
GO

IF NOT EXISTS (SELECT 1 FROM Card WHERE CardNumber = '4532015112830366')
BEGIN
    INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV,
                      CardType, CardBrand, Status, DailyTransactionLimit,
                      MonthlySpendingCap, IsContactlessEnabled, IsOnlineEnabled, SortOrder)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000'),
        (SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'),
        '4532015112830366',
        'JOHN DOE',
        '2027-12-31',
        '123',
        'Debit',
        'Visa',
        'Active',
        5000.00,
        15000.00,
        1,
        1,
        0
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Card WHERE CardNumber = '5425233430109903')
BEGIN
    INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV,
                      CardType, CardBrand, Status, DailyTransactionLimit,
                      MonthlySpendingCap, IsContactlessEnabled, IsOnlineEnabled, SortOrder)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840001'),
        (SELECT Id FROM [User] WHERE Email = 'john.doe@bankapp.com'),
        '5425233430109903',
        'JOHN DOE',
        '2026-08-31',
        '456',
        'Credit',
        'Mastercard',
        'Active',
        10000.00,
        30000.00,
        1,
        1,
        1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Card WHERE CardNumber = '4916338506082832')
BEGIN
    INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV,
                      CardType, CardBrand, Status, DailyTransactionLimit,
                      MonthlySpendingCap, IsContactlessEnabled, IsOnlineEnabled, SortOrder)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840002'),
        (SELECT Id FROM [User] WHERE Email = 'jane.smith@bankapp.com'),
        '4916338506082832',
        'JANE SMITH',
        '2028-03-31',
        '789',
        'Debit',
        'Visa',
        'Active',
        3000.00,
        10000.00,
        1,
        1,
        0
    );
END
GO