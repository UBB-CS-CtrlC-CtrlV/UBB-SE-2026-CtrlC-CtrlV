// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using System.Data.Common;
using BankApp.Server.DataAccess;
using Microsoft.Data.SqlClient;
using Respawn;
using Testcontainers.MsSql;

namespace BankApp.Server.Tests.Integration.Infrastructure;

/// <summary>
/// Provides a repeatable SQL Server database via Testcontainers for integration tests.
/// Each test class that implements <see cref="IClassFixture{DatabaseFixture}"/>
/// shares one MsSqlContainer for the lifetime of that class.
/// Call <see cref="ResetAsync"/> before each test to wipe all data cleanly.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer dbContainer;
    private DbConnection? connection;
    private Respawner? respawner;

    public DatabaseFixture()
    {
        this.dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    /// <summary>
    /// Creates a fresh <see cref="AppDbContext"/> connected to the Testcontainers SQL Server instance.
    /// </summary>
    /// <returns>A new <see cref="AppDbContext"/>.</returns>
    public AppDbContext CreateDbContext()
    {
        return new AppDbContext(this.dbContainer.GetConnectionString());
    }

    /// <summary>
    /// Wipes all data from the database using Respawn.
    /// Should be called before each test run to ensure a clean state.
    /// </summary>
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
        await this.dbContainer.StartAsync();

        this.connection = new SqlConnection(this.dbContainer.GetConnectionString());
        await this.connection.OpenAsync();

        await this.ApplySchemaAsync();

        this.respawner = await Respawner.CreateAsync(this.connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" },
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

        await this.dbContainer.DisposeAsync();
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
                using var cmd = this.connection!.CreateCommand();
                cmd.CommandText = statement;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    private static string FindSchemaDirectory()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
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

    private static IEnumerable<string> SplitSqlBatches(string sql)
    {
        var batches = new List<string>();
        var currentBatch = new List<string>();

        using var reader = new StringReader(sql);
        string? line;
        while ((line = reader.ReadLine()) is not null)
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
}
