using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebApplication1
{
    public class SqlServerDiagnostic
    {
        private readonly string _connectionString;

        public SqlServerDiagnostic(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<SqlDiagnosticResult> DiagnoseConnection()
        {
            var result = new SqlDiagnosticResult();

            try
            {
                // Teste básico de conexão
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    result.CanConnect = true;
                    result.ServerVersion = connection.ServerVersion;
                    result.ServerName = connection.DataSource;
                    result.DatabaseName = connection.Database;

                    // Testar se o banco existe e tem tabelas
                    using (var command = new SqlCommand(
                        @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                          WHERE TABLE_TYPE = 'BASE TABLE'", connection))
                    {
                        var tablesCount = (int)await command.ExecuteScalarAsync();
                        result.TablesCount = tablesCount;
                    }

                    // Testar se as tabelas principais têm dados
                    try
                    {
                        using (var command = new SqlCommand("SELECT COUNT(*) FROM Users", connection))
                        {
                            var usersCount = (int)await command.ExecuteScalarAsync();
                            result.UsersCount = usersCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.UsersError = ex.Message;
                    }

                    try
                    {
                        using (var command = new SqlCommand("SELECT COUNT(*) FROM Products", connection))
                        {
                            var productsCount = (int)await command.ExecuteScalarAsync();
                            result.ProductsCount = productsCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ProductsError = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.CanConnect = false;
                result.Error = ex.Message;
                result.InnerError = ex.InnerException?.Message;
            }

            return result;
        }
    }

    public class SqlDiagnosticResult
    {
        public bool CanConnect { get; set; }
        public string Error { get; set; } = string.Empty;
        public string InnerError { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public int TablesCount { get; set; }
        public int UsersCount { get; set; }
        public string UsersError { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public string ProductsError { get; set; } = string.Empty;
    }
}
