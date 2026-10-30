using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Race_System_2_.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegistrationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Registrations/Index (Real-time Enrolments/Registrations)
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var registrations = new List<RegistrationViewModel>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";

            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/api/Registrations");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var data = await JsonSerializer.DeserializeAsync<List<RegistrationViewModel>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (data != null) registrations = data;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Unable to load registrations: {ex.Message}";
            }

            return View(registrations);
        }
    }

    public class RegistrationViewModel
    {
        public int Id { get; set; }
        public string AthleteName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime EnroledDate { get; set; }
    }
}