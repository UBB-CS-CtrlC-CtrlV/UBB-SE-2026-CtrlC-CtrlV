# Database Scripts

## Setup
For ease of development I recommend setting up SQL Server LocalDB instead of the normal SQL Server.

Use these scripts to set up the local SQL Server database for `MovieApp`.

If your SQL client supports SQLCMD mode, run:

- `000-bootstrap.sql`

Otherwise, run these files in order:

1. `001-create-database.sql`
2. `002-create-schema.sql`
3. `003-seed-dummy-user.sql`
4. `004-create-event.sql`
5. `005-create-participation.sql`
6. `006-create-favorite-events.sql`
7. `006-user-spins.sql`
8. `007-create-movies.sql`
9. `007-create-notifications.sql`
10. `008-create-user-movie-discounts.sql`
11. `009-create-marathon.sql`
12. `010-seed-events.sql`

Notes:

- `000-bootstrap.sql` is the canonical local setup path and includes all current scripts.
- Some script numbers are duplicated because they were added in separate PRs. Follow the filenames exactly as listed above.
- `010-seed-events.sql` is idempotent and safe to rerun.
