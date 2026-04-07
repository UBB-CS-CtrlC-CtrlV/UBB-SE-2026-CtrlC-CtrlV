USE BankAppDb;
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-001-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, MerchantName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000'),
        (SELECT Id FROM Card WHERE CardNumber = '4532015112830366'),
        'TXN-001-2024', 'Purchase', 'Out',
        45.50, 'RON', 15704.50, 'Mega Image',
        (SELECT Id FROM Category WHERE Name = 'Food & Dining'),
        'Grocery shopping', 0.00, 'Completed',
        DATEADD(DAY, -1, GETUTCDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-002-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, MerchantName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000'),
        (SELECT Id FROM Card WHERE CardNumber = '4532015112830366'),
        'TXN-002-2024', 'Purchase', 'Out',
        120.00, 'RON', 15584.50, 'Uber',
        (SELECT Id FROM Category WHERE Name = 'Transportation'),
        'Ride to airport', 0.00, 'Completed',
        DATEADD(DAY, -2, GETUTCDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-003-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, CounterpartyName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000'),
        NULL,
        'TXN-003-2024', 'Credit', 'In',
        5000.00, 'RON', 20584.50, 'Employer SRL',
        (SELECT Id FROM Category WHERE Name = 'Salary'),
        'Monthly salary', 0.00, 'Completed',
        DATEADD(DAY, -5, GETUTCDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-004-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, MerchantName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000'),
        (SELECT Id FROM Card WHERE CardNumber = '4532015112830366'),
        'TXN-004-2024', 'Purchase', 'Out',
        299.99, 'RON', 20284.51, 'eMAG',
        (SELECT Id FROM Category WHERE Name = 'Shopping'),
        'Electronics purchase', 0.00, 'Completed',
        DATEADD(DAY, -7, GETUTCDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-005-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, MerchantName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840000'),
        (SELECT Id FROM Card WHERE CardNumber = '4532015112830366'),
        'TXN-005-2024', 'Purchase', 'Out',
        55.00, 'RON', 20229.51, 'Netflix',
        (SELECT Id FROM Category WHERE Name = 'Entertainment'),
        'Monthly subscription', 0.00, 'Completed',
        DATEADD(DAY, -10, GETUTCDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-006-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, MerchantName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840002'),
        (SELECT Id FROM Card WHERE CardNumber = '4916338506082832'),
        'TXN-006-2024', 'Purchase', 'Out',
        200.00, 'RON', 8720.50, 'Farmacia Catena',
        (SELECT Id FROM Category WHERE Name = 'Healthcare'),
        'Medication', 0.00, 'Completed',
        DATEADD(DAY, -3, GETUTCDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'TXN-007-2024')
BEGIN
    INSERT INTO [Transaction] (AccountId, CardId, TransactionRef, [Type], Direction,
                                Amount, Currency, BalanceAfter, CounterpartyName,
                                CategoryId, [Description], Fee, Status, CreatedAt)
    VALUES (
        (SELECT Id FROM Account WHERE IBAN = 'RO49AAAA1B31007593840002'),
        NULL,
        'TXN-007-2024', 'Credit', 'In',
        3500.00, 'RON', 12220.50, 'Employer SRL',
        (SELECT Id FROM Category WHERE Name = 'Salary'),
        'Monthly salary', 0.00, 'Completed',
        DATEADD(DAY, -5, GETUTCDATE())
    );
END
GO