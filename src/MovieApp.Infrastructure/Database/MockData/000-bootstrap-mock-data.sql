USE [MovieApp];
GO

-- Optional demo-data entrypoint.
-- Run this only after the schema and baseline scripts in Database/Scripts have been applied.
-- The files below add broader mock coverage for UI demos without changing the core schema setup.

:r .\001-seed-users-and-events.sql
:r .\002-seed-catalog-and-trivia.sql
:r .\003-seed-engagement-and-rewards.sql
:r .\004-seed-screenings-and-marathons.sql
