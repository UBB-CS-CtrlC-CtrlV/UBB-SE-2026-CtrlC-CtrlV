// <copyright file="DatabaseFixture.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Data.Common;
using BankApp.Infrastructure.DataAccess;
using Microsoft.Data.SqlClient;
using Respawn;
using Testcontainers.MsSql;

namespace BankApp.Infrastructure.Tests.Integration.Infrastructure;

/// <summary>
/// Provides a repeatable SQL Server database via Testcontainers for integration tests.
/// Each test class that implements <see cref="IClassFixture{DatabaseFixture}"/>
/// shares one MsSqlContainer for the lifetime of that class.
/// Call <see cref="ResetAsync"/> before each test to wipe all data cleanly.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global — xUnit instantiates fixtures via reflection.
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer databaseContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    private DbConnection? connection;
    private Respawner? respawner;

    /// <summary>
    /// Creates a fresh <see cref="AppDatabaseContext"/> connected to the Testcontainers SQL Server instance.
    /// </summary>
    /// <returns>A new <see cref="AppDatabaseContext"/>.</returns>
    public AppDatabaseContext CreateDatabaseContext()
    {
        return new AppDatabaseContext(this.databaseContainer.GetConnectionString());
    }

    /// <summary>
    /// Wipes all data from the database using Respawn.
    /// Should be called before each test run to ensure a clean state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ResetAsync()
    {
        if (this.respawner != null && this.connection != null)
        {
            await this.respawner.ResetAsync(this.connection);
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await this.databaseContainer.StartAsync();

        this.connection = new SqlConnection(this.databaseContainer.GetConnectionString());
        await this.connection.OpenAsync();

        await this.ApplySchemaAsync();

        const string schemaName = "dbo";

        this.respawner = await Respawner.CreateAsync(
            this.connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = new[] { schemaName },
            });
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (this.connection != null)
        {
            await this.connection.CloseAsync();
            await this.connection.DisposeAsync();
        }

        await this.databaseContainer.DisposeAsync();
    }

    private static string FindSchemaDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "scripts", "db", "schema");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate scripts/db/schema from the test output directory.");
    }

    private static IEnumerable<string> SplitSqlBatches(string databaseCommandText)
    {
        var batches = new List<string>();
        var currentBatch = new List<string>();

        using var reader = new StringReader(databaseCommandText);
        while (reader.ReadLine() is { } line)
        {
            if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
            {
                AddCurrentBatch(batches, currentBatch);
                currentBatch.Clear();
                continue;
            }

            currentBatch.Add(line);
        }

        AddCurrentBatch(batches, currentBatch);
        return batches;
    }

    private static void AddCurrentBatch(List<string> batches, List<string> currentBatch)
    {
        string batch = string.Join(Environment.NewLine, currentBatch).Trim();
        if (!string.IsNullOrWhiteSpace(batch))
        {
            batches.Add(batch);
        }
    }

    private async Task ApplySchemaAsync()
    {
        string schemaDirectory = FindSchemaDirectory();
        string[] schemaFiles = Directory.GetFiles(schemaDirectory, "*.sql")
            .Where(path => !Path.GetFileName(path).Equals("00_Database.sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (string schemaFile in schemaFiles)
        {
            foreach (string statement in SplitSqlBatches(await File.ReadAllTextAsync(schemaFile)))
            {
                await using DbCommand databaseCommand = this.connection!.CreateCommand();
                databaseCommand.CommandText = statement;
                await databaseCommand.ExecuteNonQueryAsync();
            }
        }
    }
}