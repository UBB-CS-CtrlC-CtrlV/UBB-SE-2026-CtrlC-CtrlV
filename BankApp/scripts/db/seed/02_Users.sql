USE BankAppDb;
GO

-- Password for both users is Test1234!
-- BCrypt hash generated with cost factor 11
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Email = 'john.doe@bankapp.com')
BEGIN
    INSERT INTO [User] (Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                        [Address], Nationality, PreferredLanguage, Is2FAEnabled,
                        Preferred2FAMethod, IsLocked, FailedLoginAttempts)
    VALUES (
        'john.doe@bankapp.com',
        '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.',
        'John Doe',
        '+40721234567',
        '1990-05-15',
        '123 Main Street, Bucharest, Romania',
        'Romanian',
        'en',
        0,
        NULL,
        0,
        0
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [User] WHERE Email = 'jane.smith@bankapp.com')
BEGIN
    INSERT INTO [User] (Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                        [Address], Nationality, PreferredLanguage, Is2FAEnabled,
                        Preferred2FAMethod, IsLocked, FailedLoginAttempts)
    VALUES (
        'jane.smith@bankapp.com',
        '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.',
        'Jane Smith',
        '+40737654321',
        '1995-08-22',
        '456 Oak Avenue, Cluj-Napoca, Romania',
        'Romanian',
        'en',
        0,
        NULL,
        0,
        0
    );
END
GO