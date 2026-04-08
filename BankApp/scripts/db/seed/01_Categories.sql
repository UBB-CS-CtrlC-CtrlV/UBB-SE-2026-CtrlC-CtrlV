IF NOT EXISTS (SELECT 1 FROM Category)
BEGIN
    INSERT INTO Category (Name, Icon, IsSystem) VALUES
    ('Food & Dining', 'food', 1),
    ('Transportation', 'car', 1),
    ('Shopping', 'shopping', 1),
    ('Entertainment', 'entertainment', 1),
    ('Healthcare', 'health', 1),
    ('Utilities', 'utilities', 1),
    ('Travel', 'travel', 1),
    ('Education', 'education', 1),
    ('Salary', 'salary', 1),
    ('Transfer', 'transfer', 1);
END
GO