using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using WebApplication1.Data;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiagnosticController> _logger;
        private readonly IWebHostEnvironment _env;

        public DiagnosticController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<DiagnosticController> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _env = env;
        }

        [HttpGet("full")]
        public async Task<IActionResult> GetFullDiagnostics()
        {
            var diagnostics = new Dictionary<string, object>();

            try
            {
                // 1. ENVIRONMENT
                diagnostics["Environment"] = _env.EnvironmentName;
                diagnostics["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                
                // 2. CONNECTION STRING RESOLUTION
                var connStringFromConfig = _configuration.GetConnectionString("DefaultConnection");
                var connStringFromDbContext = _context.Database.GetConnectionString();
                
                diagnostics["ConnectionString_FromConfig"] = MaskPassword(connStringFromConfig);
                diagnostics["ConnectionString_FromDbContext"] = MaskPassword(connStringFromDbContext);
                diagnostics["ConnectionStrings_Match"] = connStringFromConfig == connStringFromDbContext;

                // 3. DATABASE IDENTITY
                using (var connection = new SqlConnection(connStringFromDbContext))
                {
                    await connection.OpenAsync();
                    
                    diagnostics["Database_ServerName"] = connection.DataSource;
                    diagnostics["Database_Name"] = connection.Database;
                    diagnostics["Database_ServerVersion"] = connection.ServerVersion;
                    
                    // Get physical file location
                    using (var cmd = new SqlCommand(@"
                        SELECT physical_name 
                        FROM sys.master_files 
                        WHERE database_id = DB_ID()", connection))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        var files = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            files.Add(reader.GetString(0));
                        }
                        diagnostics["Database_PhysicalFiles"] = files;
                    }
                }

                // 4. TABLE COUNTS
                var tableCounts = new Dictionary<string, int>();
                tableCounts["Users"] = await _context.Users.CountAsync();
                tableCounts["Products"] = await _context.Products.CountAsync();
                tableCounts["Categories"] = await _context.Categories.CountAsync();
                tableCounts["Orders"] = await _context.Orders.CountAsync();
                tableCounts["Cart"] = await _context.Cart.CountAsync();
                tableCounts["Favorites"] = await _context.Favorites.CountAsync();
                tableCounts["WeeklyMenu"] = await _context.WeeklyMenu.CountAsync();
                tableCounts["Tickets"] = await _context.Tickets.CountAsync();
                
                diagnostics["TableCounts"] = tableCounts;

                // 5. SAMPLE DATA WITH TIMESTAMPS
                var sampleProducts = await _context.Products
                    .AsNoTracking()
                    .Take(3)
                    .Select(p => new { p.Id, p.Name, p.Price, p.ImageUrl, p.Stock })
                    .ToListAsync();
                
                diagnostics["SampleProducts"] = sampleProducts;

                // 6. DIRECT SQL QUERY TO VERIFY
                var directSqlProducts = new List<object>();
                using (var connection = new SqlConnection(connStringFromDbContext))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("SELECT TOP 3 Id, Name, Price, ImageUrl, Stock FROM Products ORDER BY Id", connection))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            directSqlProducts.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                ImageUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Stock = reader.GetInt32(4)
                            });
                        }
                    }
                }
                diagnostics["DirectSQL_SampleProducts"] = directSqlProducts;

                // 7. CHECK FOR MULTIPLE DATABASES
                var allDatabases = new List<string>();
                using (var connection = new SqlConnection(connStringFromDbContext))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("SELECT name FROM sys.databases WHERE name LIKE '%Refeitorio%'", connection))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            allDatabases.Add(reader.GetString(0));
                        }
                    }
                }
                diagnostics["AllRefeitorioDBs"] = allDatabases;

                // 8. PROCESS INFO
                diagnostics["ProcessId"] = Process.GetCurrentProcess().Id;
                diagnostics["MachineName"] = Environment.MachineName;
                diagnostics["Timestamp_UTC"] = DateTime.UtcNow;
                diagnostics["Timestamp_Local"] = DateTime.Now;

                return Ok(diagnostics);
            }
            catch (Exception ex)
            {
                diagnostics["Error"] = ex.Message;
                diagnostics["InnerError"] = ex.InnerException?.Message;
                diagnostics["StackTrace"] = ex.StackTrace;
                return StatusCode(500, diagnostics);
            }
        }

        [HttpGet("verify-update")]
        public async Task<IActionResult> VerifyDatabaseUpdate()
        {
            try
            {
                // Force clear EF cache
                _context.ChangeTracker.Clear();

                // Get current timestamp from database
                string dbTimestamp;
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("SELECT GETDATE()", connection))
                    {
                        dbTimestamp = ((DateTime)await cmd.ExecuteScalarAsync()).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }
                }

                // Get product count via EF
                var efCount = await _context.Products.CountAsync();

                // Get product count via direct SQL
                int sqlCount;
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Products", connection))
                    {
                        sqlCount = (int)await cmd.ExecuteScalarAsync();
                    }
                }

                return Ok(new
                {
                    DatabaseTimestamp = dbTimestamp,
                    ProductCount_EF = efCount,
                    ProductCount_DirectSQL = sqlCount,
                    CountsMatch = efCount == sqlCount,
                    ConnectionString = MaskPassword(_context.Database.GetConnectionString()),
                    Message = "If you just updated the database and counts don't match, there's a cache or connection issue"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, InnerError = ex.InnerException?.Message });
            }
        }

        private string MaskPassword(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;
            
            return connectionString
                .Replace("Password=", "Password=***")
                .Replace("Pwd=", "Pwd=***");
        }
    }
}
