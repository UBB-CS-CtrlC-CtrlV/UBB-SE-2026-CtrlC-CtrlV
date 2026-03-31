using Microsoft.Data.SqlClient;
using System.Data;
namespace BankApp.Server.DataAccess
{
    public class AppDbContext : IDbContext
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;
        private SqlTransaction? _currentTransaction;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                try
                {
                    _connection = new SqlConnection(_connectionString);
                    _connection.Open();
                }
                catch (SqlException e)
                {
                    throw new Exception($"Failed to connect to the database: {e.Message}", e);
                }
            }
            return _connection;
        }

        public SqlTransaction BeginTransaction()
        {
            SqlConnection conn = GetConnection();
            try
            {
                _currentTransaction = conn.BeginTransaction();
            }
            catch (SqlException e)
            {
                throw new Exception($"Failed to begin transaction: {e.Message}", e);
            }
            return _currentTransaction;
        }

        public void CommitTransaction()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Commit();
                _currentTransaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Rollback();
                _currentTransaction = null;
            }
        }

        public SqlTransaction? GetCurrentTransaction()
        {
            return _currentTransaction;
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

        public IDataReader ExecuteQuery(string sqlStatement, object[] parameters)
        {
            var conn = GetConnection();
            var cmd = new SqlCommand(sqlStatement, conn, _currentTransaction);
            AddParameters(cmd, parameters);
            return cmd.ExecuteReader(); // returns rows back
        }

        public int ExecuteNonQuery(string sqlStatement, object[] parameters)
        {
            var conn = GetConnection();
            using var cmd = new SqlCommand(sqlStatement, conn, _currentTransaction); // disposes the command when done with it
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery(); // how many rows are affected
        }

        public void Dispose()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
            }

            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}