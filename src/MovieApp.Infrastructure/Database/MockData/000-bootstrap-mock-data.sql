USE [MovieApp];
GO

-- Optional demo-data entrypoint.
-- Run this only after the schema bootstrap in Database/Scripts/000-bootstrap.sql.

:r .\001-seed-dummy-user.sql
:r .\002-seed-base-events.sql
:r .\003-seed-base-trivia-questions.sql
:r .\004-seed-base-movies-and-cast.sql
:r .\005-seed-base-user-spins.sql
:r .\006-seed-base-marathons.sql
:r .\007-seed-extra-users-and-events.sql
:r .\008-seed-extra-catalog-and-trivia.sql
:r .\009-seed-engagement-and-rewards.sql
:r .\010-seed-screenings-and-marathons.sql
:r .\011-seed-marathons.sql
:r .\012-seed-marathon-testing-data.sql
