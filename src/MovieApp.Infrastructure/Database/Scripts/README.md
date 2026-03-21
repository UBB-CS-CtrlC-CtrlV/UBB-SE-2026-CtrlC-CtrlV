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
7. `007-create-notifications.sql`

The scripts are safe to rerun.
