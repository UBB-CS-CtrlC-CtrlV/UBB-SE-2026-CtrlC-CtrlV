# Database Scripts

Use these scripts to set up the local SQL Server database for `MovieApp`.

If your SQL client supports SQLCMD mode, run:

- `000-initialize-all.sql`

Otherwise, run these files in order:

1. `001-create-database.sql`
2. `002-create-schema.sql`
3. `003-seed-dummy-user.sql`

The scripts are safe to rerun.
