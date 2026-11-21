using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using refeitorio.ViewModels;

namespace refeitorio.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        public async Task<UserViewModel> Login(string userNumber, string password)
        {
            try
            {
                var loginData = new
                {
                    UserNumber = userNumber,
                    Password = password
                };

                var jsonContent = JsonSerializer.Serialize(loginData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/Auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserViewModel>();
                }
            }
            catch
            {
                // API unreachable: treat as failed login (no local fallback).
            }

            return null;
        }
        
        public async Task<UserViewModel?> Register(string userNumber, string password, string name, string? email = null, string? phone = null)
        {
            try
            {
                var registerData = new
                {
                    UserNumber = userNumber,
                    Password = password,
                    Name = name,
                    Email = email ?? "",
                    Phone = phone ?? "",
                    UserType = "Student"
                };

                var jsonContent = JsonSerializer.Serialize(registerData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.PostAsync($"{_baseUrl}/Auth/register", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        return JsonSerializer.Deserialize<UserViewModel>(responseContent, new JsonSerializerOptions {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
            }
            catch { }
            
            return null;
        }

        public async Task<UserViewModel> GetUserById(int id)
        {
            try
            {
                
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync($"{_baseUrl}/Users/{id}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        return JsonSerializer.Deserialize<UserViewModel>(responseContent, new JsonSerializerOptions {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                
                // Se falhar a API, tentar criar um usuário local para os IDs conhecidos
                if (id == 1 || id == 2 || id == 3)
                {
                    if (id == 1)
                    {
                        return new UserViewModel
                        {
                            Id = 1,
                            UserNumber = "admin",
                            Name = "ADMIN",
                            UserType = "Admin",
                            ProfileImage = ""
                        };
                    }
                    else if (id == 2)
                    {
                        return new UserViewModel
                        {
                            Id = 2,
                            UserNumber = "student",
                            Name = "STUDENT", 
                            UserType = "Student",
                            ProfileImage = ""
                        };
                    }
                    else if (id == 3)
                    {
                        return new UserViewModel
                        {
                            Id = 3,
                            UserNumber = "func",
                            Name = "FUNCIONÁRIO",
                            UserType = "Staff",
                            ProfileImage = ""
                        };
                    }
                }
            }
            catch { }
            
            return null;
        }

        public async Task<bool> AddBalance(int userId, decimal amount)
        {
            try
            {
                
                // Serialize with InvariantCulture to ensure decimal uses dot separator
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false
                };
                var jsonContent = JsonSerializer.Serialize(amount, options);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.PostAsync($"{_baseUrl}/Users/{userId}/addbalance", content);
                    
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProfileImage(int userId, string imageUrl)
        {
            try
            {
                
                var jsonContent = JsonSerializer.Serialize(imageUrl);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.PutAsync($"{_baseUrl}/Users/{userId}/profileimage", content);
                    
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
