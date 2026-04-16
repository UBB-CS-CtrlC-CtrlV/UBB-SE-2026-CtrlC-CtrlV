-- Migration: enforce valid NotificationPreference categories
--
-- Before NotificationType display names were established, the seed and early
-- registrations wrote arbitrary strings ('Transactions', 'Security', etc.)
-- that do not correspond to any known NotificationType value.
--
-- This migration:
--   1. Removes rows whose Category is not a recognised display name.
--      These rows are unreadable by the application and cannot be repaired
--      because the old values have no meaningful mapping to current types.
--   2. Adds a CHECK constraint so the database rejects invalid values going
--      forward.  The constraint matches the NotificationType enum display names
--      defined in NotificationTypeExtensions.ToDisplayName().

-- Step 1: remove unrecognised rows (idempotent — safe to re-run).
DELETE FROM NotificationPreference
WHERE Category NOT IN (
    'Payment',
    'Inbound Transfer',
    'Outbound Transfer',
    'Low Balance',
    'Due Payment',
    'Suspicious Activity'
);
GO

-- Step 2: add the CHECK constraint if it does not already exist.
IF NOT EXISTS (
    SELECT 1
    FROM   sys.check_constraints
    WHERE  name              = 'CK_NotificationPreference_Category'
      AND  parent_object_id = OBJECT_ID('dbo.NotificationPreference')
)
BEGIN
    ALTER TABLE NotificationPreference
    ADD CONSTRAINT CK_NotificationPreference_Category CHECK (Category IN (
        'Payment',
        'Inbound Transfer',
        'Outbound Transfer',
        'Low Balance',
        'Due Payment',
        'Suspicious Activity'
    ));
END
GO
