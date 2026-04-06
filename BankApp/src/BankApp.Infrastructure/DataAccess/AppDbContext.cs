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
                catch (SqlException sqlException)
                {
                    throw new Exception($"Failed to connect to the database: {sqlException.Message}", sqlException);
                }
            }
            return connection;
        }

        /// <inheritdoc />
        public virtual IDbTransaction BeginTransaction()
        {
            SqlConnection activeConnection = GetConnection();
            try
            {
                currentTransaction = activeConnection.BeginTransaction();
            }
            catch (SqlException sqlException)
            {
                throw new Exception($"Failed to begin transaction: {sqlException.Message}", sqlException);
            }
            return currentTransaction;
        }

        /// <inheritdoc />
        public virtual void CommitTransaction()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Commit();
                currentTransaction = null;
            }
        }

        /// <inheritdoc />
        public virtual void RollbackTransaction()
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

        private void AddParameters(SqlCommand command, object[] parameters)
        {
            if (parameters == null)
            {
                return;
            }
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                command.Parameters.AddWithValue($"@p{parameterIndex}", parameters[parameterIndex] ?? DBNull.Value);
            }
        }

        /// <inheritdoc />
        public virtual IDataReader ExecuteQuery(string sqlStatement, object[] parameters)
        {
            var activeConnection = GetConnection();
            var command = new SqlCommand(sqlStatement, activeConnection, currentTransaction);
            AddParameters(command, parameters);
            return command.ExecuteReader();
        }

        /// <inheritdoc />
        public virtual int ExecuteNonQuery(string sqlStatement, object[] parameters)
        {
            var activeConnection = GetConnection();
            using var command = new SqlCommand(sqlStatement, activeConnection, currentTransaction);
            AddParameters(command, parameters);
            return command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public virtual void Dispose()
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
