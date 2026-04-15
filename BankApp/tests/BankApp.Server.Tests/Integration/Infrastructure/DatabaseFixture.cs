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
        if (respawner != null && connection != null)
        {
            await respawner.ResetAsync(connection);
        }
    }

    public async Task InitializeAsync()
    {
        await this.dbContainer.StartAsync();

        this.connection = new SqlConnection(this.dbContainer.GetConnectionString());
        await this.connection.OpenAsync();

        await this.ApplySchemaAsync();

        this.respawner = await Respawner.CreateAsync(this.connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" }
        });
    }

    public async Task DisposeAsync()
    {
        if (connection != null)
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
        await this.dbContainer.DisposeAsync();
    }

    // ─── Schema Creation ──────────────────────────────────────────────────────

    private async Task ApplySchemaAsync()
    {
        // SQL Scripts should be loaded from the output directory (preserved via csproj)
        var scripts = Directory.GetFiles(AppContext.BaseDirectory, "*.sql")
            .OrderBy(f => f) // Assumes files are prefixed with numbers like 00_..., 01_...
            .ToList();

        if (!scripts.Any())
        {
            throw new FileNotFoundException("No SQL schema scripts found in the output directory. Make sure they are copied upon build.");
        }

        foreach (var scriptPath in scripts)
        {
            string scriptText = await File.ReadAllTextAsync(scriptPath);
            using var cmd = this.connection!.CreateCommand();
            
            // Basic script splitting via GO statement, since ADO.NET doesn't support GO directly
            string[] commands = scriptText.Split(new[] { "\nGO", "\r\nGO", "\ngo", "\r\ngo" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var commandText in commands)
            {
                if (string.IsNullOrWhiteSpace(commandText))
                {
                    continue;
                }
                cmd.CommandText = commandText;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
