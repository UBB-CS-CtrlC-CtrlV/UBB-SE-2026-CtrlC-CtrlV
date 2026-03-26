# Database Scripts

## Setup
For ease of development I recommend setting up SQL Server LocalDB instead of the normal SQL Server.

Use these scripts to set up the local SQL Server database for `MovieApp`.

If your SQL client supports SQLCMD mode, run:

- `000-bootstrap.sql`
- `..\MockData\000-bootstrap-mock-data.sql` if you also want demo/mock rows

Otherwise, run these files in order:

1. `000-clear-db.sql`
2. `001-create-database.sql`
3. `002-create-schema.sql`
4. `004-create-event.sql`
5. `005-create-participation.sql`
6. `006-create-favorite-events.sql`
7. `007-create-movies.sql`
8. `008-user-spins.sql`
9. `009-create-notifications.sql`
10. `010-create-user-movie-discounts.sql`
11. `011-create-marathon.sql`
12. `013-create-trivia-questions.sql`
13. `014-create-ambassador-profile.sql`
14. `015-create-referral-log.sql`
15. `016-create-screenings.sql`
16. `019-create-trivia-rewards.sql`
17. `..\MockData\000-bootstrap-mock-data.sql` for optional demo/mock data

Notes:

- `000-bootstrap.sql` is the canonical schema bootstrap and intentionally excludes mock data.
- All mock/demo rows now live under `Database/MockData`.
- The old `seed-*.sql` files left in `Scripts` are handoff stubs so existing paths still explain where the data moved.
