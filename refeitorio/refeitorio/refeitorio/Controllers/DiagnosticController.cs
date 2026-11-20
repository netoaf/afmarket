using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using refeitorio.Services;
using refeitorio.ViewModels;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace refeitorio.Controllers
{
    public class DiagnosticController : Controller
    {
        private readonly DiagnosticService _diagnosticService;
        private readonly HttpClientService _httpClientService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(
            DiagnosticService diagnosticService,
            HttpClientService httpClientService,
            IConfiguration configuration,
            ILogger<DiagnosticController> logger)
        {
            _diagnosticService = diagnosticService;
            _httpClientService = httpClientService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = apiBaseUrl;
            
            try
            {
                var apiHealth = await _diagnosticService.CheckApiConnectivity();
                ViewBag.ApiStatus = apiHealth;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro no diagnóstico: {ex.Message}");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet("TestSql")]
        public async Task<IActionResult> TestSqlConnection()
        {
            return View("SqlTest");        
        }
        
        [HttpGet("SqlTest")]
        public IActionResult SqlTest()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }
        
        [HttpGet("SqlDiagnostics")]
        public async Task<IActionResult> SqlDiagnostics()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            
            try
            {
                // Testando SQL via nosso endpoint TestDb na API
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/TestDb");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);
                        ViewBag.SqlStatus = new
                        {
                            Success = true,
                            Message = "API e SQL estão funcionando",
                            Details = content
                        };
                    }
                    else
                    {
                        ViewBag.SqlStatus = new
                        {
                            Success = false,
                            Message = $"API respondeu com status {response.StatusCode}",
                            Details = await response.Content.ReadAsStringAsync()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao testar SQL: {ex.Message}");
                ViewBag.SqlStatus = new
                {
                    Success = false,
                    Message = $"Erro ao conectar à API: {ex.Message}",
                    Details = ex.StackTrace
                };
            }
            
            // Verificar se a API está rodando
            try
            {
                var apiProcess = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.Contains("WebApplication1") || 
                                                                           p.ProcessName.Contains("Refeitorio_API") ||
                                                                           p.ProcessName.Contains("dotnet"));
                
                ViewBag.ApiProcess = apiProcess != null
                    ? $"API parece estar rodando (PID: {apiProcess.Id})"
                    : "API não parece estar em execução";
            }
            catch (Exception ex)
            {
                ViewBag.ApiProcess = $"Erro ao verificar processo da API: {ex.Message}";
            }
            
            return View("Index");
        }

        [HttpGet("FullTrace")]
        public async Task<IActionResult> FullTrace()
        {
            var trace = new Dictionary<string, object>();

            try
            {
                // 1. MVC Configuration
                trace["MVC_Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                trace["MVC_ApiBaseUrl_Config"] = _configuration["ApiSettings:BaseUrl"];
                trace["MVC_ProcessId"] = Process.GetCurrentProcess().Id;
                trace["MVC_Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // 2. Call API Diagnostic endpoint
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Diagnostic/full";
                    
                    _logger.LogInformation($"[MVC-TRACE] Calling API diagnostic: {apiUrl}");
                    
                    var response = await client.GetAsync(apiUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        trace["API_Diagnostics"] = JsonConvert.DeserializeObject<dynamic>(content);
                    }
                    else
                    {
                        trace["API_Diagnostics_Error"] = new
                        {
                            StatusCode = response.StatusCode,
                            Content = await response.Content.ReadAsStringAsync()
                        };
                    }
                }

                // 3. Test actual product fetch
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var productsUrl = $"{_configuration["ApiSettings:BaseUrl"]}/Products";
                    
                    _logger.LogInformation($"[MVC-TRACE] Fetching products from: {productsUrl}");
                    
                    var response = await client.GetAsync(productsUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var products = JsonConvert.DeserializeObject<List<ProductViewModel>>(content);
                        
                        trace["Products_Received"] = new
                        {
                            Count = products?.Count ?? 0,
                            Sample = products?.Take(3).Select(p => new { p.Id, p.Name, p.Price, p.ImageUrl, p.Stock })
                        };
                    }
                    else
                    {
                        trace["Products_Error"] = new
                        {
                            StatusCode = response.StatusCode,
                            Content = await response.Content.ReadAsStringAsync()
                        };
                    }
                }

                return Json(trace);
            }
            catch (Exception ex)
            {
                trace["Error"] = ex.Message;
                trace["InnerError"] = ex.InnerException?.Message;
                trace["StackTrace"] = ex.StackTrace;
                return Json(trace);
            }
        }
    }
}
