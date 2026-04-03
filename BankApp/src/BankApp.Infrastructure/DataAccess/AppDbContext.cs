using System.Data;
using Microsoft.Data.SqlClient;

namespace BankApp.Infrastructure.DataAccess
{
    /// <summary>
    /// Provides a concrete implementation of <see cref="IDbContext"/> using SQL Server via <see cref="SqlConnection"/>.
    /// </summary>
    public class AppDbContext : IDbContext
    {
        private readonly string connectionString;
        private SqlConnection? connection;
        private SqlTransaction? currentTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class.
        /// </summary>
        /// <param name="connectionString">The SQL Server connection string.</param>
        public AppDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets an open <see cref="SqlConnection"/>, creating one if necessary.
        /// </summary>
        /// <returns>An open <see cref="SqlConnection"/> instance.</returns>
        public SqlConnection GetConnection()
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                }
                catch (SqlException e)
                {
                    throw new Exception($"Failed to connect to the database: {e.Message}", e);
                }
            }
            return connection;
        }

        /// <inheritdoc />
        public SqlTransaction BeginTransaction()
        {
            SqlConnection conn = GetConnection();
            try
            {
                currentTransaction = conn.BeginTransaction();
            }
            catch (SqlException e)
            {
                throw new Exception($"Failed to begin transaction: {e.Message}", e);
            }
            return currentTransaction;
        }

        /// <inheritdoc />
        public void CommitTransaction()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Commit();
                currentTransaction = null;
            }
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Rollback();
                currentTransaction = null;
            }
        }

        /// <summary>
        /// Gets the currently active transaction, or <see langword="null"/> if none exists.
        /// </summary>
        /// <returns>The current <see cref="SqlTransaction"/>, or <see langword="null"/>.</returns>
        public SqlTransaction? GetCurrentTransaction()
        {
            return currentTransaction;
        }

        private void AddParameters(SqlCommand cmd, object[] parameters)
        {
            if (parameters == null)
            {
                return;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
            }
        }

        /// <inheritdoc />
        public IDataReader ExecuteQuery(string sqlStatement, object[] parameters)
        {
            var conn = GetConnection();
            var cmd = new SqlCommand(sqlStatement, conn, currentTransaction);
            AddParameters(cmd, parameters);
            return cmd.ExecuteReader(); // returns rows back
        }

        /// <inheritdoc />
        public int ExecuteNonQuery(string sqlStatement, object[] parameters)
        {
            var conn = GetConnection();
            using var cmd = new SqlCommand(sqlStatement, conn, currentTransaction); // disposes the command when done with it
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery(); // how many rows are affected
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Dispose();
            }

            if (connection != null)
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }

                connection.Dispose();
                connection = null;
            }
        }
    }
}
