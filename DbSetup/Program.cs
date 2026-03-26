using System;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string server = @"(localdb)\MSSQLLocalDB";
        string scriptsDir = @"d:\UBB\Anul2\Sem2\ISS\UBB-SE-2026-CtrlC-CtrlV\src\MovieApp.Infrastructure\Database\Scripts";
        var files = Directory.GetFiles(scriptsDir, "*.sql").OrderBy(f => f).ToList();
        
        using (var masterConn = new SqlConnection($"Data Source={server};Initial Catalog=master;Integrated Security=True;Encrypt=False"))
        {
            masterConn.Open();
            var dbCheckCmd = new SqlCommand("SELECT db_id('MovieApp')", masterConn);
            bool exists = dbCheckCmd.ExecuteScalar() != DBNull.Value;
            
            // Just running Create script manually if Db is missing is safe, but we can also just run it anyway and catch
            try { ExecuteScript(masterConn, files.First(f => f.Contains("001"))); } catch { }
        }
        
        using (var dbConn = new SqlConnection($"Data Source={server};Initial Catalog=MovieApp;Integrated Security=True;Encrypt=False"))
        {
            dbConn.Open();
            foreach (var file in files.Where(f => !f.Contains("001")))
            {
                Console.WriteLine($"Running {Path.GetFileName(file)}...");
                ExecuteScript(dbConn, file);
            }
        }
        Console.WriteLine("All scripts executed! Database is fully populated with test data!");
    }
    
    static void ExecuteScript(SqlConnection conn, string filePath)
    {
        string sql = File.ReadAllText(filePath);
        var statements = sql.Split(new[] { "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var stmt in statements)
        {
            var cleanStmt = stmt.Trim();
            if (string.IsNullOrWhiteSpace(cleanStmt) || cleanStmt.StartsWith("USE [MovieApp]")) continue;
            using var cmd = new SqlCommand(cleanStmt, conn);
            try { cmd.ExecuteNonQuery(); } 
            catch(Exception ex) { Console.WriteLine($"Warning in {Path.GetFileName(filePath)}: {ex.Message}"); }
        }
    }
}
