-- Canonical schema bootstrap entrypoint.
-- This clears the local database and recreates the schema only.

:r .\000-clear-db.sql
:r .\001-create-database.sql
:r .\002-create-schema.sql
:r .\004-create-event.sql
:r .\005-create-participation.sql
:r .\006-create-favorite-events.sql
:r .\007-create-movies.sql
:r .\008-user-spins.sql
:r .\009-create-notifications.sql
:r .\010-create-user-movie-discounts.sql
:r .\011-create-marathon.sql
:r .\013-create-trivia-questions.sql
:r .\014-create-ambassador-profile.sql
:r .\015-create-referral-log.sql
:r .\016-create-screenings.sql
:r .\019-create-trivia-rewards.sql
