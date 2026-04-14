// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using System.Data;
using System.Text.RegularExpressions;
using BankApp.Server.DataAccess;
using Dapper;
using ErrorOr;
using Microsoft.Data.Sqlite;

namespace BankApp.Server.Tests.Integration.Infrastructure;

/// <summary>
/// A test-only <see cref="AppDbContext"/> that uses a SQLite in-memory connection.
/// Intercepts every query lambda to translate T-SQL syntax into SQLite-compatible SQL.
/// All DataAccess classes work unchanged — only the SQL strings are rewritten.
/// </summary>
internal sealed class SqliteDbContext : AppDbContext
{
    private readonly SqliteConnection sqliteConnection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDbContext"/> class.
    /// </summary>
    /// <param name="connection">The open SQLite in-memory connection to use.</param>
    public SqliteDbContext(SqliteConnection connection)
        : base(":memory:")  // Dummy connection string — we override Query<T> entirely
    {
        this.sqliteConnection = connection;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This override wraps the original <paramref name="operation"/> with an
    /// <see cref="IDbConnection"/> proxy that intercepts every SQL command
    /// and translates T-SQL syntax to SQLite before execution.
    /// </remarks>
    public override ErrorOr<T> Query<T>(Func<IDbConnection, T> operation)
    {
        try
        {
            return operation(new TranslatingConnection(this.sqliteConnection));
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    // ─── SQL Translation ─────────────────────────────────────────────────────

    /// <summary>
    /// Translates T-SQL specific syntax to SQLite-compatible equivalents.
    /// </summary>
    internal static string TranslateSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return sql;
        }

        // Capture OUTPUT INSERTED columns for later SELECT reconstruction
        bool hasOutputInserted = Regex.IsMatch(sql, @"\bOUTPUT\s+INSERTED\.", RegexOptions.IgnoreCase);

        // Remove OUTPUT INSERTED.col1, INSERTED.col2, ... clause entirely
        sql = Regex.Replace(
            sql,
            @"\s*OUTPUT\s+INSERTED\.\w+(?:\s*,\s*INSERTED\.\w+)*\s*",
            " ",
            RegexOptions.IgnoreCase);

        // GETUTCDATE() → datetime('now')
        sql = Regex.Replace(sql, @"\bGETUTCDATE\(\)", "datetime('now')", RegexOptions.IgnoreCase);

        // DATEADD(DAY, @Param, datetime('now')) → datetime('now', '+7 days')
        // We simplify: any DATEADD(DAY, @*, ...) → datetime('now', '+7 days')
        sql = Regex.Replace(
            sql,
            @"\bDATEADD\s*\(\s*DAY\s*,\s*@(\w+)\s*,\s*datetime\('now'\)\s*\)",
            "datetime('now', '+7 days')",
            RegexOptions.IgnoreCase);

        // SELECT TOP (@Limit) ... FROM ... ORDER BY ... → SELECT ... FROM ... ORDER BY ... LIMIT @Limit
        var topMatch = Regex.Match(sql, @"\bSELECT\s+TOP\s*\(@(\w+)\)\s*", RegexOptions.IgnoreCase);
        if (topMatch.Success)
        {
            string paramName = topMatch.Groups[1].Value;
            sql = Regex.Replace(
                sql,
                @"\bSELECT\s+TOP\s*\(@\w+\)\s*",
                "SELECT ",
                RegexOptions.IgnoreCase);

            // Append LIMIT at the very end
            sql = sql.TrimEnd(' ', '\r', '\n', ';') + $"\nLIMIT @{paramName}";
        }

        // [TableName] or [ColumnName] bracket identifiers → "TableName" / "ColumnName"
        sql = Regex.Replace(sql, @"\[(\w+)\]", "\"$1\"");

        return sql.Trim();
    }

    /// <summary>
    /// Determines whether the SQL contains an OUTPUT INSERTED clause.
    /// </summary>
    internal static bool IsInsertWithOutput(string sql) =>
        Regex.IsMatch(sql, @"\bOUTPUT\s+INSERTED\.", RegexOptions.IgnoreCase);

    // ─── Proxy Connection ─────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a <see cref="SqliteConnection"/> and intercepts every <see cref="IDbCommand"/>
    /// creation to transparently translate SQL. Dapper calls <c>CreateCommand()</c>
    /// internally, so this is the correct interception point.
    /// </summary>
    private sealed class TranslatingConnection : IDbConnection
    {
        private readonly SqliteConnection inner;

        public TranslatingConnection(SqliteConnection inner)
        {
            this.inner = inner;
        }

#pragma warning disable CS8767
        public string ConnectionString
        {
            get => this.inner.ConnectionString ?? string.Empty;
            set => this.inner.ConnectionString = value ?? string.Empty;
        }
#pragma warning restore CS8767

        public int ConnectionTimeout => this.inner.ConnectionTimeout;

        public string Database => this.inner.Database;

        public ConnectionState State => this.inner.State;

        public IDbTransaction BeginTransaction() => this.inner.BeginTransaction();

        public IDbTransaction BeginTransaction(IsolationLevel il) => this.inner.BeginTransaction(il);

        public void ChangeDatabase(string databaseName) => this.inner.ChangeDatabase(databaseName);

        public void Close() => this.inner.Close();

        public void Open() => this.inner.Open();

        public void Dispose() { /* do not dispose the shared connection */ }

        public IDbCommand CreateCommand()
        {
            return new TranslatingCommand(this.inner.CreateCommand(), this.inner);
        }
    }

    /// <summary>
    /// Wraps a <see cref="SqliteCommand"/> and translates the SQL on every execution.
    /// For INSERT...OUTPUT INSERTED queries, executes in two steps:
    /// 1. The INSERT (without OUTPUT clause)
    /// 2. SELECT * FROM table WHERE rowid = last_insert_rowid()
    /// </summary>
    private sealed class TranslatingCommand : IDbCommand
    {
        private readonly SqliteCommand inner;
        private readonly SqliteConnection connection;
        private string commandText = string.Empty;

        public TranslatingCommand(SqliteCommand inner, SqliteConnection connection)
        {
            this.inner = inner;
            this.connection = connection;
        }

#pragma warning disable CS8767
        public string CommandText
        {
            get => this.commandText;
            set => this.commandText = value ?? string.Empty;
        }
#pragma warning restore CS8767

        public int CommandTimeout
        {
            get => this.inner.CommandTimeout;
            set => this.inner.CommandTimeout = value;
        }

        public CommandType CommandType
        {
            get => this.inner.CommandType;
            set => this.inner.CommandType = value;
        }

        public IDbConnection? Connection
        {
            get => this.inner.Connection;
            set => this.inner.Connection = (SqliteConnection?)value;
        }

        public IDataParameterCollection Parameters => this.inner.Parameters;

        public IDbTransaction? Transaction
        {
            get => this.inner.Transaction;
            set => this.inner.Transaction = (SqliteTransaction?)value;
        }

        public UpdateRowSource UpdatedRowSource
        {
            get => this.inner.UpdatedRowSource;
            set => this.inner.UpdatedRowSource = value;
        }

        public void Cancel() => this.inner.Cancel();

        public IDbDataParameter CreateParameter() => this.inner.CreateParameter();

        public void Dispose() => this.inner.Dispose();

        public void Prepare() => this.inner.Prepare();

        public int ExecuteNonQuery()
        {
            this.inner.CommandText = TranslateSql(this.commandText);
            return this.inner.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader() => this.ExecuteReader(CommandBehavior.Default);

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            bool originallyHadOutput = IsInsertWithOutput(this.commandText);
            string translated = TranslateSql(this.commandText);

            if (originallyHadOutput)
            {
                // Step 1: Run the INSERT (OUTPUT clause already stripped by TranslateSql)
                this.inner.CommandText = translated;
                this.inner.ExecuteNonQuery();

                // Step 2: SELECT the row we just inserted by last_insert_rowid()
                // Try bracket-translated name first ("TableName"), then fallback to unquoted
                var tableMatch = Regex.Match(
                    translated,
                    @"INSERT\s+INTO\s+(?:""(\w+)""|(\w+))",
                    RegexOptions.IgnoreCase);

                string tableName = tableMatch.Success
                    ? (tableMatch.Groups[1].Success ? tableMatch.Groups[1].Value : tableMatch.Groups[2].Value)
                    : throw new InvalidOperationException($"Cannot determine table name from INSERT: {translated}");

                // Do NOT use 'using' here — Dapper reads the open reader after this method returns
                var selectCmd = this.connection.CreateCommand();
                selectCmd.CommandText = $"SELECT * FROM \"{tableName}\" WHERE rowid = last_insert_rowid()";
                return selectCmd.ExecuteReader(behavior);
            }

            this.inner.CommandText = translated;
            return this.inner.ExecuteReader(behavior);
        }

        public object? ExecuteScalar()
        {
            this.inner.CommandText = TranslateSql(this.commandText);
            return this.inner.ExecuteScalar();
        }
    }
}
