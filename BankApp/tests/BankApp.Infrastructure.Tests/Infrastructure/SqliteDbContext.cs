// <copyright file="SqliteDbContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using BankApp.Infrastructure.DataAccess;

namespace BankApp.Infrastructure.Tests.Infrastructure;

/// <summary>
/// SQLite-backed test double for <see cref="AppDbContext"/>.
/// Overrides <c>ExecuteQuery</c> / <c>ExecuteNonQuery</c> to translate
/// SQL Server dialect (TOP, OUTPUT INSERTED, GETUTCDATE, DATEADD, [brackets])
/// to SQLite-compatible syntax, so all production DataAccess classes work
/// without any modification.
/// </summary>
public class SqliteDbContext : AppDbContext
{
    private SqliteConnection? sqliteConnection;
    private SqliteTransaction? sqliteTransaction;

    // Reserved SQLite identifiers that need quoting
    private static readonly HashSet<string> ReservedWords =
        new(StringComparer.OrdinalIgnoreCase) { "User", "Session", "Transaction", "Order", "Group" };

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDbContext"/> class
    /// connected to an in-memory SQLite database.
    /// </summary>
    /// <param name="connectionString">SQLite connection string (e.g. <c>Data Source=:memory:</c>).</param>
    public SqliteDbContext(string connectionString)
        : base("Server=test-dummy;Database=test;")   // Dummy – base never opens a connection (methods overridden)
    {
        sqliteConnection = new SqliteConnection(connectionString);
        sqliteConnection.Open();
    }

    // ── Schema helpers ──────────────────────────────────────────────────────

    /// <summary>Runs a batch of DDL statements separated by semicolons.</summary>
    public void ExecuteSchema(string schemaSql)
    {
        // Split on semicolons at statement boundaries
        foreach (var raw in schemaSql.Split(';'))
        {
            var stmt = raw.Trim();
            if (stmt.Length == 0)
            {
                continue;
            }

            using var cmd = sqliteConnection!.CreateCommand();
            cmd.CommandText = stmt;
            cmd.Transaction = sqliteTransaction;
            cmd.ExecuteNonQuery();
        }
    }

    // ── IDbContext overrides ────────────────────────────────────────────────

    /// <inheritdoc/>
    public override IDbTransaction BeginTransaction()
    {
        sqliteTransaction = sqliteConnection!.BeginTransaction();
        return sqliteTransaction;
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
        sqliteTransaction?.Commit();
        sqliteTransaction = null;
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
        sqliteTransaction?.Rollback();
        sqliteTransaction = null;
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteQuery(string sqlStatement, object[] parameters)
    {
        if (TryHandleOutputInserted(sqlStatement, parameters, out IDataReader? reader))
        {
            return reader!;
        }

        string translated = TranslateToSqlite(sqlStatement);
        return ExecuteQueryInternal(translated, parameters);
    }

    /// <inheritdoc/>
    public override int ExecuteNonQuery(string sqlStatement, object[] parameters)
    {
        string translated = TranslateToSqlite(sqlStatement);
        return ExecuteNonQueryInternal(translated, parameters);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        sqliteTransaction?.Dispose();
        sqliteTransaction = null;

        if (sqliteConnection != null)
        {
            sqliteConnection.Close();
            sqliteConnection.Dispose();
            sqliteConnection = null;
        }
    }

    // ── Internal helpers ────────────────────────────────────────────────────

    private IDataReader ExecuteQueryInternal(string sql, object[] parameters)
    {
        var cmd = sqliteConnection!.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = sqliteTransaction;
        AddSqliteParameters(cmd, parameters);
        return cmd.ExecuteReader();
    }

    private int ExecuteNonQueryInternal(string sql, object[] parameters)
    {
        using var cmd = sqliteConnection!.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = sqliteTransaction;
        AddSqliteParameters(cmd, parameters);
        return cmd.ExecuteNonQuery();
    }

    private static void AddSqliteParameters(SqliteCommand cmd, object[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            cmd.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
        }
    }

    // ── OUTPUT INSERTED translation ─────────────────────────────────────────

    /// <summary>
    /// Handles "INSERT INTO [Table] (...) OUTPUT INSERTED.Col1, ... VALUES (...)"
    /// by running a plain INSERT then SELECT-ing the row via last_insert_rowid().
    /// </summary>
    private bool TryHandleOutputInserted(
        string sql,
        object[] parameters,
        out IDataReader? reader)
    {
        // Pattern captures: (1) table name, (2) OUTPUT column list, presence of VALUES
        var match = Regex.Match(
            sql,
            @"INSERT\s+INTO\s+\[?(\w+)\]?\s*(?:\([^)]*\))?\s+OUTPUT\s+((?:INSERTED\.\w+(?:\s*,\s*)?)+)\s+VALUES",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!match.Success)
        {
            reader = null;
            return false;
        }

        string tableName = match.Groups[1].Value;
        string outputClause = match.Groups[2].Value;

        // Extract column names: "INSERTED.Id, INSERTED.UserId" → ["Id", "UserId"]
        string[] columns = Regex.Matches(outputClause, @"INSERTED\.(\w+)", RegexOptions.IgnoreCase)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToArray();

        // Strip OUTPUT clause → plain INSERT
        string insertOnly = Regex.Replace(
            sql,
            @"\s+OUTPUT\s+(?:INSERTED\.\w+(?:\s*,\s*)?)+\s+VALUES",
            " VALUES",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        string translatedInsert = TranslateToSqlite(insertOnly);
        ExecuteNonQueryInternal(translatedInsert, parameters);

        // SELECT the inserted row by rowid
        string quotedTable = QuoteIfReserved(tableName);
        string selectSql = $"SELECT {string.Join(", ", columns)} FROM {quotedTable} WHERE rowid = last_insert_rowid()";
        reader = ExecuteQueryInternal(selectSql, Array.Empty<object>());
        return true;
    }

    // ── SQL dialect translation ─────────────────────────────────────────────

    private static string TranslateToSqlite(string sql)
    {
        // 1. [identifier] → "identifier" (for reserved words)
        sql = Regex.Replace(sql, @"\[(\w+)\]", m =>
        {
            string name = m.Groups[1].Value;
            return QuoteIfReserved(name);
        });

        // 2. GETUTCDATE() → datetime('now')
        sql = Regex.Replace(sql, @"GETUTCDATE\(\)", "datetime('now')", RegexOptions.IgnoreCase);

        // 3. DATEADD(DAY, @pN, datetime('now')) → datetime('now', '+' || CAST(@pN AS TEXT) || ' days')
        sql = Regex.Replace(
            sql,
            @"DATEADD\s*\(\s*DAY\s*,\s*(@p\d+)\s*,\s*datetime\('now'\)\s*\)",
            "datetime('now', '+' || CAST($1 AS TEXT) || ' days')",
            RegexOptions.IgnoreCase);

        // 4. SELECT TOP (@pN) ... ORDER BY x → SELECT ... ORDER BY x LIMIT @pN
        var topMatch = Regex.Match(sql, @"TOP\s*\(\s*(@p\d+)\s*\)\s+", RegexOptions.IgnoreCase);
        if (topMatch.Success)
        {
            string topParam = topMatch.Groups[1].Value;
            sql = sql.Remove(topMatch.Index, topMatch.Length);
            sql = sql.TrimEnd(' ', '\n', '\r', '\t', ';') + $" LIMIT {topParam}";
        }

        return sql;
    }

    private static string QuoteIfReserved(string name)
        => ReservedWords.Contains(name) ? $"\"{name}\"" : name;
}
